using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Google.Protobuf;
using Google.Protobuf.Collections;
using OpenTelemetry.Proto.Trace.V1;

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
        return spanQueryRequest.Filters.Count == 0 || spanQueryRequest.Filters.All(whereFilter => SpanMatchesWhereFilter(whereFilter, span));
    }

    private static bool SpanMatchesWhereFilter(WhereSpanFilter filter, Span span) => filter.FilterCase switch
    {
        WhereSpanFilter.FilterOneofCase.SpanProperty => ProcessSpanPropertyFilter(filter.SpanProperty, span),
        _ => throw new NotImplementedException("Something went wrong"),
    };

    

    private static bool ProcessSpanPropertyFilter(WhereSpanPropertyFilter filter, Span span)
    {
        return filter.PropertyCase switch
        {
            WhereSpanPropertyFilter.PropertyOneofCase.SpanName => StringFilter(span.Name, filter.SpanName.Compare, filter.SpanName.CompareAs),
            WhereSpanPropertyFilter.PropertyOneofCase.SpanId => ByteStringFilter(span.SpanId, filter.SpanId.Compare, filter.SpanId.CompareAs),
            WhereSpanPropertyFilter.PropertyOneofCase.TraceId => ByteStringFilter(span.TraceId, filter.TraceId.Compare, filter.TraceId.CompareAs),
            WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId => ByteStringFilter(span.ParentSpanId, filter.ParentSpanId.Compare, filter.ParentSpanId.CompareAs),
            WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano => UInt64Filter(span.StartTimeUnixNano, filter.StartTimeUnixNano.Compare, filter.StartTimeUnixNano.CompareAs),
            WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano => UInt64Filter(span.EndTimeUnixNano, filter.EndTimeUnixNano.Compare, filter.EndTimeUnixNano.CompareAs),
            WhereSpanPropertyFilter.PropertyOneofCase.SpanStatusCode => StatusCodeFilter(span.Status.Code, filter.SpanStatusCode.Compare, filter.SpanStatusCode.CompareAs),
            WhereSpanPropertyFilter.PropertyOneofCase.SpanKind => KindFilter(span.Kind, filter.SpanKind.Compare, filter.SpanKind.CompareAs),
            WhereSpanPropertyFilter.PropertyOneofCase.SpanAttribute => KeyValueFilter(span.Attributes, filter.SpanAttribute)
        };
    }

    private static bool StringFilter(string value, string compare, StringCompareAsType compareType)
    {
        return compareType switch
        {
            StringCompareAsType.Equals => value.Equals(compare, StringComparison.Ordinal),
            StringCompareAsType.NotEquals => !value.Equals(compare, StringComparison.Ordinal),
            StringCompareAsType.Contains => value.Contains(compare),
            StringCompareAsType.NotContains => !value.Contains(compare),
            StringCompareAsType.IsEmpty => string.IsNullOrEmpty(value),
            StringCompareAsType.IsNotEmpty => !string.IsNullOrEmpty(value),
            StringCompareAsType.None => throw new NotImplementedException("Something went wrong"), // TODO update this exception
            _ => throw new NotImplementedException("Something went wrong"),
        };
    }

    private static bool ByteStringFilter(ByteString value, ByteString compare, ByteStringCompareAsType compareType)
    {
        return compareType switch
        {
            ByteStringCompareAsType.Equals => value.Equals(compare),
            ByteStringCompareAsType.NotEquals => !value.Equals(compare),
            ByteStringCompareAsType.Empty => value.IsEmpty,
            ByteStringCompareAsType.NotEmpty => !value.IsEmpty
        };
    }

    private static bool UInt64Filter(ulong value, ulong compare, UInt64CompareAsType compareType)
    {
        return compareType switch
        {
            UInt64CompareAsType.Equals => value.Equals(compare),
            UInt64CompareAsType.NotEquals => !value.Equals(compare),
            UInt64CompareAsType.GreaterThanEquals => value >= compare,
            UInt64CompareAsType.GreaterThan => value > compare,
            UInt64CompareAsType.LessThanEquals => value <= compare,
            UInt64CompareAsType.LessThan => value < compare,
        };
    }
    
    private static bool Int64Filter(long value, long compare, Int64CompareAsType compareType)
    {
        return compareType switch
        {
            Int64CompareAsType.Equals => value.Equals(compare),
        };
    }

    private static bool StatusCodeFilter(SpanStatusCode value, SpanStatusCode compare,
        EnumCompareAsType compareAsType)
    {
        return compareAsType switch
        {
            EnumCompareAsType.Equals => value.Equals(compare),
            EnumCompareAsType.NotEquals => !value.Equals(compare),
        };
    }

    private static bool KindFilter(SpanKind value, SpanKind compare, EnumCompareAsType compareAsType)
    {
        return compareAsType switch
        {
            EnumCompareAsType.Equals => value.Equals(compare),
            EnumCompareAsType.NotEquals => !value.Equals(compare),
        };
    }

    private static bool KeyValueFilter(MapField<string, AnyValue> map, KeyValueProperty property)
    {
        if (map.TryGetValue(property.Key, out var value))
        {
            return property.ValueCase switch
            {
                KeyValueProperty.ValueOneofCase.StringValue => StringFilter(value.StringValue,
                    property.StringValue.Compare, property.StringValue.CompareAs),
                KeyValueProperty.ValueOneofCase.ByteStringValue => ByteStringFilter(value.BytesValue, 
                    property.ByteStringValue.Compare, property.ByteStringValue.CompareAs),
                KeyValueProperty.ValueOneofCase.Uint64Value => UInt64Filter(value.UIntValue, 
                    property.Uint64Value.Compare, property.Uint64Value.CompareAs),
                KeyValueProperty.ValueOneofCase.Int64Value => Int64Filter(value.IntValue, 
                    property.Int64Value.Compare, property.Int64Value.CompareAs)
            };
        }

        return false;
    }
}