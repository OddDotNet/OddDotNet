using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Google.Protobuf;

namespace OddDotNet;

public class SpanSignalList : ISignalList<Span>
{
    private readonly IChannelManager<Span> _channels;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SpanSignalList> _logger;

    private static readonly List<Expirable<Span>> Spans = [];

    private static readonly object Lock = new();
    public SpanSignalList(IChannelManager<Span> channels, TimeProvider timeProvider, ILogger<SpanSignalList> logger)
    {
        _channels = channels;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public void Add(Span signal)
    {
        lock (Lock)
        {
            PruneExpiredSpans();
            
            // Add the new span with 30 second expire
            // TODO make this configurable
            DateTimeOffset expiresAt = _timeProvider.GetUtcNow().AddSeconds(30);
            Spans.Add(new Expirable<Span>(signal, expiresAt));
            
            // Notify any listening channels
            _channels.NotifyChannels(signal);
        }
    }

    public async IAsyncEnumerable<Span> QueryAsync(IQueryRequest<Span> request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        SpanQueryRequest spanRequest = request as SpanQueryRequest ?? throw new InvalidCastException(nameof(request));
        using var timeout = GetQueryTimeout(spanRequest);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
        
        Channel<Span> channel = _channels.AddChannel();

        try
        {
            // Create the channel and populate it with the current contents of the span list
            lock (Lock)
            {
                PruneExpiredSpans();

                foreach (var expirableSpan in Spans)
                {
                    channel.Writer.TryWrite(expirableSpan.Signal);
                }
            }

            int takeCount = GetTakeCount(spanRequest);
            int currentCount = 0;

            while (currentCount < takeCount && !cts.IsCancellationRequested)
            {
                Span? span = null;
                try
                {
                    await channel.Reader.WaitToReadAsync(cts.Token);
                    span = await channel.Reader.ReadAsync(cts.Token);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogWarning(ex, "The query operation was cancelled");
                    break;
                }
                

                if (ShouldInclude(spanRequest, span))
                {
                    yield return span;
                    currentCount++;
                }
                    
            }
        }
        finally
        {
            _channels.RemoveChannel(channel);
            channel.Writer.TryComplete();
        }
    }

    private static CancellationTokenSource GetQueryTimeout(SpanQueryRequest spanRequest)
    {
        var defaultTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(int.MaxValue));
        
        return spanRequest.Duration is null ? 
            defaultTimeout : 
            spanRequest.Duration?.ValueCase switch
        {
            Duration.ValueOneofCase.MillisecondsValue => new CancellationTokenSource(
                TimeSpan.FromMilliseconds(spanRequest.Duration.MillisecondsValue)),
            Duration.ValueOneofCase.SecondsValue => new CancellationTokenSource(
                TimeSpan.FromSeconds(spanRequest.Duration.SecondsValue)),
            Duration.ValueOneofCase.MinutesValue => new CancellationTokenSource(
                TimeSpan.FromMinutes(spanRequest.Duration.MinutesValue)),
            Duration.ValueOneofCase.None => defaultTimeout,
            _ => defaultTimeout
        };
    }

    private void PruneExpiredSpans()
    {
        DateTimeOffset currentTime = _timeProvider.GetUtcNow();
        Spans.RemoveAll(expirable => expirable.ExpireAt < currentTime);
    }

    private static int GetTakeCount(SpanQueryRequest spanQueryRequest) => spanQueryRequest.Take.TakeTypeCase switch
    {
        Take.TakeTypeOneofCase.TakeFirst => 1,
        Take.TakeTypeOneofCase.TakeAll => int.MaxValue,
        Take.TakeTypeOneofCase.TakeExact => spanQueryRequest.Take.TakeExact.Count,
        Take.TakeTypeOneofCase.None => throw new Exception("Take type invalid"), // TODO change to better exception,
        _ => throw new Exception("Take type invalid") // TODO change to better exception
    };

    private static bool ShouldInclude(SpanQueryRequest spanQueryRequest, Span span)
    {
        return spanQueryRequest.WhereFilters.Count == 0 || spanQueryRequest.WhereFilters.All(whereFilter => SpanMatchesWhereFilter(whereFilter, span));
    }

    private static bool SpanMatchesWhereFilter(Where filter, Span span) => filter.FilterCase switch
    {
        Where.FilterOneofCase.AttributeStringEqual => ProcessWhereAttributeStringEqualFilter(
            filter.AttributeStringEqual, span),
        Where.FilterOneofCase.AttributeIntEqual => ProcessWhereAttributeIntEqualFilter(filter.AttributeIntEqual, span),
        Where.FilterOneofCase.AttributeExists => ProcessWhereAttributeExistsFilter(filter.AttributeExists, span),
        Where.FilterOneofCase.SpanProperty => ProcessSpanPropertyFilter(filter.SpanProperty, span),
        _ => throw new NotImplementedException("Something went wrong"),
    };

    private static bool ProcessWhereAttributeStringEqualFilter(WhereAttributeStringEqualFilter filter, Span span)
    {
        if (!span.Attributes.TryGetValue(filter.Attribute, out var attribute)) 
            return false;
            
        return attribute.HasStringValue && string.Equals(attribute.StringValue, filter.Compare, StringComparison.Ordinal);
    }

    private static bool ProcessWhereAttributeIntEqualFilter(WhereAttributeIntEqualFilter filter, Span span)
    {
        if (!span.Attributes.TryGetValue(filter.Attribute, out var attribute))
            return false;

        return attribute.HasIntValue && filter.Compare == attribute.IntValue;
    }

    private static bool ProcessWhereAttributeExistsFilter(WhereAttributeExistsFilter filter, Span span)
    {
        return span.Attributes.ContainsKey(filter.Attribute);
    }

    private static bool ProcessSpanPropertyFilter(WhereSpanPropertyFilter filter, Span span)
    {
        return filter.PropertyCase switch
        {
            WhereSpanPropertyFilter.PropertyOneofCase.SpanName => StringFilter(span.Name, filter.SpanName.Compare, filter.SpanName.CompareAs),
            WhereSpanPropertyFilter.PropertyOneofCase.SpanId => ByteStringFilter(span.SpanId, filter.SpanId.Compare, filter.SpanId.CompareAs)
        };
    }

    private static bool StringFilter(string value, string compare, StringCompareType compareType)
    {
        return compareType switch
        {
            StringCompareType.Equals => value.Equals(compare, StringComparison.Ordinal),
            StringCompareType.NotEquals => !value.Equals(compare, StringComparison.Ordinal),
            StringCompareType.Contains => value.Contains(compare),
            StringCompareType.NotContains => !value.Contains(compare),
            StringCompareType.IsEmpty => string.IsNullOrEmpty(value),
            StringCompareType.IsNotEmpty => !string.IsNullOrEmpty(value),
            StringCompareType.None => throw new NotImplementedException("Something went wrong"), // TODO update this exception
            _ => throw new NotImplementedException("Something went wrong"),
        };
    }

    private static bool ByteStringFilter(ByteString value, ByteString compare, ByteStringCompareType compareType)
    {
        return compareType switch
        {
            ByteStringCompareType.Equals => value.Equals(compare),
            ByteStringCompareType.NotEquals => !value.Equals(compare),
        };
    }
}