using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Google.Protobuf;
using Google.Protobuf.Collections;
using OddDotNet.Proto.Spans.V1;

// using OpenTelemetry.Proto.Trace.V1;

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

    public void Reset(IResetRequest<Span> request)
    {
        lock (Lock)
        {
            Spans.Clear();
        }
    }

    private static CancellationTokenSource GetQueryTimeout(SpanQueryRequest spanRequest)
    {
        var defaultTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(int.MaxValue));

        return spanRequest.Duration is null
            ? defaultTimeout
            : new CancellationTokenSource(TimeSpan.FromMilliseconds(spanRequest.Duration.Milliseconds));
    }

    private void PruneExpiredSpans()
    {
        DateTimeOffset currentTime = _timeProvider.GetUtcNow();
        Spans.RemoveAll(expirable => expirable.ExpireAt < currentTime);
    }

    private static int GetTakeCount(SpanQueryRequest spanQueryRequest) => spanQueryRequest.Take.ValueCase switch
    {
        Take.ValueOneofCase.TakeFirst => 1,
        Take.ValueOneofCase.TakeAll => int.MaxValue,
        Take.ValueOneofCase.TakeExact => spanQueryRequest.Take.TakeExact.Count,
        Take.ValueOneofCase.None => throw new Exception("Take type invalid"), // TODO change to better exception,
        _ => throw new Exception("Take type invalid") // TODO change to better exception
    };

    private static bool ShouldInclude(SpanQueryRequest spanQueryRequest, Span span)
    {
        return spanQueryRequest.Filters.Count == 0 || spanQueryRequest.Filters.All(whereFilter => SpanMatchesWhereFilter(whereFilter, span));
    }

    private static bool SpanMatchesWhereFilter(WhereSpanFilter filter, Span span) => filter.ValueCase switch
    {
        WhereSpanFilter.ValueOneofCase.SpanProperty => ProcessSpanPropertyFilter(filter.SpanProperty, span),
        WhereSpanFilter.ValueOneofCase.SpanOr => filter.SpanOr.Filters.Any(whereFilter => SpanMatchesWhereFilter(whereFilter, span)),
        _ => throw new NotImplementedException("Something went wrong"),
    };

    

    private static bool ProcessSpanPropertyFilter(WhereSpanPropertyFilter filter, Span span)
    {
        return filter.ValueCase switch
        {
            WhereSpanPropertyFilter.ValueOneofCase.Name => StringFilter(span.Name, filter.Name.Compare, filter.Name.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.TraceState => StringFilter(span.TraceState, filter.TraceState.Compare, filter.TraceState.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.SpanId => ByteStringFilter(span.SpanId, filter.SpanId.Compare, filter.SpanId.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.TraceId => ByteStringFilter(span.TraceId, filter.TraceId.Compare, filter.TraceId.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.ParentSpanId => ByteStringFilter(span.ParentSpanId, filter.ParentSpanId.Compare, filter.ParentSpanId.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano => UInt64Filter(span.StartTimeUnixNano, filter.StartTimeUnixNano.Compare, filter.StartTimeUnixNano.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.EndTimeUnixNano => UInt64Filter(span.EndTimeUnixNano, filter.EndTimeUnixNano.Compare, filter.EndTimeUnixNano.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.StatusCode => StatusCodeFilter(span.Status.Code, filter.StatusCode.Compare, filter.StatusCode.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.Kind => KindFilter(span.Kind, filter.Kind.Compare, filter.Kind.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.Attribute => KeyValueFilter(span.Attributes, filter.Attribute),
            WhereSpanPropertyFilter.ValueOneofCase.Flags => UInt32Filter(span.Flags, filter.Flags.Compare, filter.Flags.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.EventTimeUnixNano => span.Events.Any(spanEvent => UInt64Filter(spanEvent.TimeUnixNano, filter.EventTimeUnixNano.Compare, filter.EventTimeUnixNano.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.EventName => span.Events.Any(spanEvent => StringFilter(spanEvent.Name, filter.EventName.Compare, filter.EventName.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkTraceId => span.Links.Any(link => ByteStringFilter(link.TraceId, filter.LinkTraceId.Compare, filter.LinkTraceId.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkSpanId => span.Links.Any(link => ByteStringFilter(link.SpanId, filter.LinkSpanId.Compare, filter.LinkSpanId.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkTraceState => span.Links.Any(link => StringFilter(link.TraceState, filter.LinkTraceState.Compare, filter.LinkTraceState.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkFlags => span.Links.Any(link => UInt32Filter(link.Flags, filter.LinkFlags.Compare, filter.LinkFlags.CompareAs)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkAttribute => span.Links.Any(link => KeyValueFilter(link.Attributes, filter.LinkAttribute)),
            WhereSpanPropertyFilter.ValueOneofCase.EventAttribute => span.Events.Any(spanEvent => KeyValueFilter(spanEvent.Attributes, filter.EventAttribute)),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeAttribute => KeyValueFilter(span.InstrumentationScope.Attributes, filter.InstrumentationScopeAttribute),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeName => StringFilter(span.InstrumentationScope.Name, filter.InstrumentationScopeName.Compare, filter.InstrumentationScopeName.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeSchemaUrl => StringFilter(span.InstrumentationScope.SchemaUrl, filter.InstrumentationScopeSchemaUrl.Compare, filter.InstrumentationScopeSchemaUrl.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeVersion => StringFilter(span.InstrumentationScope.Version, filter.InstrumentationScopeVersion.Compare, filter.InstrumentationScopeVersion.CompareAs),
            WhereSpanPropertyFilter.ValueOneofCase.ResourceAttribute => KeyValueFilter(span.InstrumentationScope.Resource.Attributes, filter.ResourceAttribute),
            WhereSpanPropertyFilter.ValueOneofCase.ResourceSchemaUrl => StringFilter(span.InstrumentationScope.Resource.SchemaUrl, filter.ResourceSchemaUrl.Compare, filter.ResourceSchemaUrl.CompareAs),
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
                KeyValueProperty.ValueOneofCase.Int64Value => Int64Filter(value.IntValue, 
                    property.Int64Value.Compare, property.Int64Value.CompareAs),
                KeyValueProperty.ValueOneofCase.BoolValue => BoolFilter(value.BoolValue, 
                    property.BoolValue.Compare, property.BoolValue.CompareAs),
                KeyValueProperty.ValueOneofCase.DoubleValue => DoubleFilter(value.DoubleValue, 
                    property.DoubleValue.Compare, property.DoubleValue.CompareAs),
            };
        }

        return false;
    }
}