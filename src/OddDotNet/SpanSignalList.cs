using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Options;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Trace.V1;


namespace OddDotNet;

public class SpanSignalList : ISignalList<FlatSpan>
{
    private readonly IChannelManager<FlatSpan> _channels;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SpanSignalList> _logger;
    private readonly OddSettings _oddSettings;

    private static readonly List<Expirable<FlatSpan>> Spans = [];

    private static readonly object Lock = new();
    public SpanSignalList(IChannelManager<FlatSpan> channels, TimeProvider timeProvider, ILogger<SpanSignalList> logger, IOptions<OddSettings> oddSettings)
    {
        _channels = channels;
        _timeProvider = timeProvider;
        _logger = logger;
        _oddSettings = oddSettings.Value;
    }

    public void Add(FlatSpan signal)
    {
        lock (Lock)
        {
            
            // Add the new span with configured expiration
            DateTimeOffset expiresAt = _timeProvider.GetUtcNow().AddMilliseconds(_oddSettings.Cache.Expiration);
            Spans.Add(new Expirable<FlatSpan>(signal, expiresAt));
            
            // Notify any listening channels
            _channels.NotifyChannels(signal);
        }
    }

    public async IAsyncEnumerable<FlatSpan> QueryAsync(IQueryRequest<FlatSpan> request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        SpanQueryRequest spanRequest = request as SpanQueryRequest ?? throw new InvalidCastException(nameof(request));
        using var timeout = GetQueryTimeout(spanRequest);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
        
        Channel<FlatSpan> channel = _channels.AddChannel();

        try
        {
            // Create the channel and populate it with the current contents of the span list
            lock (Lock)
            {
                foreach (var expirableSpan in Spans)
                {
                    channel.Writer.TryWrite(expirableSpan.Signal);
                }
            }

            int takeCount = GetTakeCount(spanRequest);
            int currentCount = 0;

            while (currentCount < takeCount && !cts.IsCancellationRequested)
            {
                FlatSpan? span = null;
                try
                {
                    await channel.Reader.WaitToReadAsync(cts.Token);
                    span = await channel.Reader.ReadAsync(cts.Token);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogDebug(ex, "The query operation was cancelled");
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

    public void Reset(IResetRequest<FlatSpan> request)
    {
        lock (Lock)
        {
            Spans.Clear();
        }
    }

    public void Prune()
    {
        _logger.LogDebug("Prune");
        lock (Lock)
        {
            DateTimeOffset currentTime = _timeProvider.GetUtcNow();
            int numRemoved = Spans.RemoveAll(expirable => expirable.ExpireAt < currentTime);
            _logger.LogDebug("Removed {numRemoved} spans", numRemoved);
        }
    }

    private static CancellationTokenSource GetQueryTimeout(SpanQueryRequest spanRequest)
    {
        var defaultTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(int.MaxValue));

        return spanRequest.Duration is null
            ? defaultTimeout
            : new CancellationTokenSource(TimeSpan.FromMilliseconds(spanRequest.Duration.Milliseconds));
    }

    // private void PruneExpiredSpans()
    // {
    //     DateTimeOffset currentTime = _timeProvider.GetUtcNow();
    //     Spans.RemoveAll(expirable => expirable.ExpireAt < currentTime);
    // }

    private static int GetTakeCount(SpanQueryRequest spanQueryRequest) => spanQueryRequest.Take.ValueCase switch
    {
        Take.ValueOneofCase.TakeFirst => 1,
        Take.ValueOneofCase.TakeAll => int.MaxValue,
        Take.ValueOneofCase.TakeExact => spanQueryRequest.Take.TakeExact.Count,
        Take.ValueOneofCase.None => throw new Exception("Take type invalid"), // TODO change to better exception,
        _ => throw new Exception("Take type invalid") // TODO change to better exception
    };

    private static bool ShouldInclude(SpanQueryRequest spanQueryRequest, FlatSpan span)
    {
        return spanQueryRequest.Filters.Count == 0 || spanQueryRequest.Filters.All(whereFilter => SpanMatchesWhereFilter(whereFilter, span));
    }

    private static bool SpanMatchesWhereFilter(WhereSpanFilter filter, FlatSpan span) => filter.ValueCase switch
    {
        WhereSpanFilter.ValueOneofCase.SpanProperty => ProcessSpanPropertyFilter(filter.SpanProperty, span),
        WhereSpanFilter.ValueOneofCase.SpanOr => filter.SpanOr.Filters.Any(whereFilter => SpanMatchesWhereFilter(whereFilter, span)),
        _ => throw new NotImplementedException("Something went wrong"),
    };

    

    private static bool ProcessSpanPropertyFilter(WhereSpanPropertyFilter filter, FlatSpan span)
    {
        return filter.ValueCase switch
        {
            WhereSpanPropertyFilter.ValueOneofCase.Name => StringFilter(span.Span.Name, filter.Name.Compare, filter.Name.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.TraceState => StringFilter(span.Span.TraceState, filter.TraceState.Compare, filter.TraceState.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.SpanId => ByteStringFilter(span.Span.SpanId, filter.SpanId.Compare, filter.SpanId.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.TraceId => ByteStringFilter(span.Span.TraceId, filter.TraceId.Compare, filter.TraceId.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.ParentSpanId => ByteStringFilter(span.Span.ParentSpanId, filter.ParentSpanId.Compare, filter.ParentSpanId.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano => UInt64Filter(span.Span.StartTimeUnixNano, filter.StartTimeUnixNano.Compare, filter.StartTimeUnixNano.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.EndTimeUnixNano => UInt64Filter(span.Span.EndTimeUnixNano, filter.EndTimeUnixNano.Compare, filter.EndTimeUnixNano.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.StatusCode => StatusCodeFilter(span.Span.Status.Code, filter.StatusCode.Compare, filter.StatusCode.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.Kind => KindFilter(span.Span.Kind, filter.Kind.Compare, filter.Kind.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.Attribute => KeyValueFilter(span.Span.Attributes, filter.Attribute),
            WhereSpanPropertyFilter.ValueOneofCase.Flags => UInt32Filter(span.Span.Flags, filter.Flags.Compare, filter.Flags.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.EventTimeUnixNano => span.Span.Events.Any(spanEvent => UInt64Filter(spanEvent.TimeUnixNano, filter.EventTimeUnixNano.Compare, filter.EventTimeUnixNano.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.EventName => span.Span.Events.Any(spanEvent => StringFilter(spanEvent.Name, filter.EventName.Compare, filter.EventName.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkTraceId => span.Span.Links.Any(link => ByteStringFilter(link.TraceId, filter.LinkTraceId.Compare, filter.LinkTraceId.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkSpanId => span.Span.Links.Any(link => ByteStringFilter(link.SpanId, filter.LinkSpanId.Compare, filter.LinkSpanId.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkTraceState => span.Span.Links.Any(link => StringFilter(link.TraceState, filter.LinkTraceState.Compare, filter.LinkTraceState.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkFlags => span.Span.Links.Any(link => UInt32Filter(link.Flags, filter.LinkFlags.Compare, filter.LinkFlags.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkAttribute => span.Span.Links.Any(link => KeyValueFilter(link.Attributes, filter.LinkAttribute)),
            WhereSpanPropertyFilter.ValueOneofCase.EventAttribute => span.Span.Events.Any(spanEvent => KeyValueFilter(spanEvent.Attributes, filter.EventAttribute)),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeAttribute => KeyValueFilter(span.InstrumentationScope.Attributes, filter.InstrumentationScopeAttribute),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeName => StringFilter(span.InstrumentationScope.Name, filter.InstrumentationScopeName.Compare, filter.InstrumentationScopeName.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeSchemaUrl => StringFilter(span.InstrumentationScopeSchemaUrl, filter.InstrumentationScopeSchemaUrl.Compare, filter.InstrumentationScopeSchemaUrl.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeVersion => StringFilter(span.InstrumentationScope.Version, filter.InstrumentationScopeVersion.Compare, filter.InstrumentationScopeVersion.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.ResourceAttribute => KeyValueFilter(span.Resource.Attributes, filter.ResourceAttribute),
            WhereSpanPropertyFilter.ValueOneofCase.ResourceSchemaUrl => StringFilter(span.ResourceSchemaUrl, filter.ResourceSchemaUrl.Compare, filter.ResourceSchemaUrl.CompareAs),
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

    private static bool UInt64Filter(ulong value, ulong compare, NumberCompareAsType compareType)
    {
        return compareType switch
        {
            NumberCompareAsType.Equals => value.Equals(compare),
            NumberCompareAsType.NotEquals => !value.Equals(compare),
            NumberCompareAsType.GreaterThanEquals => value >= compare,
            NumberCompareAsType.GreaterThan => value > compare,
            NumberCompareAsType.LessThanEquals => value <= compare,
            NumberCompareAsType.LessThan => value < compare,
        };
    }
    
    private static bool UInt32Filter(uint value, uint compare, NumberCompareAsType compareType)
    {
        return compareType switch
        {
            NumberCompareAsType.Equals => value.Equals(compare),
            NumberCompareAsType.NotEquals => !value.Equals(compare),
            NumberCompareAsType.GreaterThanEquals => value >= compare,
            NumberCompareAsType.GreaterThan => value > compare,
            NumberCompareAsType.LessThanEquals => value <= compare,
            NumberCompareAsType.LessThan => value < compare,
        };
    }
    
    private static bool Int64Filter(long value, long compare, NumberCompareAsType compareType)
    {
        return compareType switch
        {
            NumberCompareAsType.Equals => value.Equals(compare),
            NumberCompareAsType.NotEquals => !value.Equals(compare),
            NumberCompareAsType.GreaterThanEquals => value >= compare,
            NumberCompareAsType.GreaterThan => value > compare,
            NumberCompareAsType.LessThanEquals => value <= compare,
            NumberCompareAsType.LessThan => value < compare,
        };
    }
    
    private static bool DoubleFilter(double value, double compare, NumberCompareAsType compareType)
    {
        return compareType switch
        {
            NumberCompareAsType.Equals => value.Equals(compare),
            NumberCompareAsType.NotEquals => !value.Equals(compare),
            NumberCompareAsType.GreaterThanEquals => value >= compare,
            NumberCompareAsType.GreaterThan => value > compare,
            NumberCompareAsType.LessThanEquals => value <= compare,
            NumberCompareAsType.LessThan => value < compare,
        };
    }
    
    private static bool BoolFilter(bool value, bool compare, BoolCompareAsType compareType)
    {
        return compareType switch
        {
            BoolCompareAsType.Equals => value.Equals(compare),
            BoolCompareAsType.NotEquals => !value.Equals(compare),
        };
    }

    private static bool StatusCodeFilter(Status.Types.StatusCode value, Status.Types.StatusCode compare,
        EnumCompareAsType compareAsType)
    {
        return compareAsType switch
        {
            EnumCompareAsType.Equals => value.Equals(compare),
            EnumCompareAsType.NotEquals => !value.Equals(compare),
        };
    }

    private static bool KindFilter(Span.Types.SpanKind value, Span.Types.SpanKind compare, EnumCompareAsType compareAsType)
    {
        return compareAsType switch
        {
            EnumCompareAsType.Equals => value.Equals(compare),
            EnumCompareAsType.NotEquals => !value.Equals(compare),
        };
    }

    private static bool KeyValueFilter(RepeatedField<KeyValue> map, KeyValueProperty property)
    {
        var keyValue = map.FirstOrDefault(kv => kv.Key == property.Key);
        if (keyValue is not null)
        {
            // TODO add support for ArrayValue and KvListValue
            return property.ValueCase switch
            {
                KeyValueProperty.ValueOneofCase.StringValue => StringFilter(keyValue.Value.StringValue,
                    property.StringValue.Compare, property.StringValue.CompareAs),
                KeyValueProperty.ValueOneofCase.ByteStringValue => ByteStringFilter(keyValue.Value.BytesValue, 
                    property.ByteStringValue.Compare, property.ByteStringValue.CompareAs),
                KeyValueProperty.ValueOneofCase.Int64Value => Int64Filter(keyValue.Value.IntValue, 
                    property.Int64Value.Compare, property.Int64Value.CompareAs),
                KeyValueProperty.ValueOneofCase.BoolValue => BoolFilter(keyValue.Value.BoolValue, 
                    property.BoolValue.Compare, property.BoolValue.CompareAs),
                KeyValueProperty.ValueOneofCase.DoubleValue => DoubleFilter(keyValue.Value.DoubleValue, 
                    property.DoubleValue.Compare, property.DoubleValue.CompareAs),
            };
        }

        return false;
    }
}