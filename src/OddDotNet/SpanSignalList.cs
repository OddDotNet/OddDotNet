using System.Runtime.CompilerServices;
using System.Threading.Channels;

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
        using var timeout = spanRequest.Take.TakeTypeCase switch
        {
            Take.TakeTypeOneofCase.TakeAll => new CancellationTokenSource(
                TimeSpan.FromSeconds(spanRequest.Take.TakeAll.Duration.SecondsValue)),
            _ => new CancellationTokenSource(TimeSpan.FromMilliseconds(Int32.MaxValue))
        };
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
                catch (OperationCanceledException e)
                {
                    // do nothing eh
                }
                

                if (span is not null && ShouldInclude(spanRequest, span))
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

    private void PruneExpiredSpans()
    {
        DateTimeOffset currentTime = _timeProvider.GetUtcNow();
        Spans.RemoveAll(expirable => expirable.ExpireAt < currentTime);
    }

    private int GetTakeCount(SpanQueryRequest spanQueryRequest) => spanQueryRequest.Take.TakeTypeCase switch
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
}