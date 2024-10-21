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
                FlatSpan? span;
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

    private static int GetTakeCount(SpanQueryRequest spanQueryRequest) => spanQueryRequest.Take.ValueCase switch
    {
        Take.ValueOneofCase.TakeFirst => 1,
        Take.ValueOneofCase.TakeAll => int.MaxValue,
        Take.ValueOneofCase.TakeExact => spanQueryRequest.Take.TakeExact.Count,
        Take.ValueOneofCase.None => 0,
        _ => 0
    };

    private static bool ShouldInclude(SpanQueryRequest spanQueryRequest, FlatSpan span)
    {
        return spanQueryRequest.Filters.Count == 0 || spanQueryRequest.Filters.All(whereFilter => SpanMatchesWhereFilter(whereFilter, span));
    }

    private static bool SpanMatchesWhereFilter(WhereSpanFilter filter, FlatSpan span) => filter.ValueCase switch
    {
        WhereSpanFilter.ValueOneofCase.SpanProperty => ProcessSpanPropertyFilter(filter.SpanProperty, span),
        WhereSpanFilter.ValueOneofCase.SpanOr => filter.SpanOr.Filters.Any(whereFilter => SpanMatchesWhereFilter(whereFilter, span)),
        WhereSpanFilter.ValueOneofCase.None => false,
        _ => false
    };

    

    private static bool ProcessSpanPropertyFilter(WhereSpanPropertyFilter filter, FlatSpan span)
    {
        return filter.ValueCase switch
        {
            WhereSpanPropertyFilter.ValueOneofCase.Name => StringFilter(span.Span.Name, filter.Name),
            WhereSpanPropertyFilter.ValueOneofCase.TraceState => StringFilter(span.Span.TraceState, filter.TraceState),
            WhereSpanPropertyFilter.ValueOneofCase.SpanId => ByteStringFilter(span.Span.SpanId, filter.SpanId),
            WhereSpanPropertyFilter.ValueOneofCase.TraceId => ByteStringFilter(span.Span.TraceId, filter.TraceId),
            WhereSpanPropertyFilter.ValueOneofCase.ParentSpanId => ByteStringFilter(span.Span.ParentSpanId, filter.ParentSpanId),
            WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano => UInt64Filter(span.Span.StartTimeUnixNano, filter.StartTimeUnixNano),
            WhereSpanPropertyFilter.ValueOneofCase.EndTimeUnixNano => UInt64Filter(span.Span.EndTimeUnixNano, filter.EndTimeUnixNano),
            WhereSpanPropertyFilter.ValueOneofCase.StatusCode => StatusCodeFilter(span.Span.Status.Code, filter.StatusCode),
            WhereSpanPropertyFilter.ValueOneofCase.Kind => KindFilter(span.Span.Kind, filter.Kind),
            WhereSpanPropertyFilter.ValueOneofCase.Attribute => KeyValueFilter(span.Span.Attributes, filter.Attribute),
            WhereSpanPropertyFilter.ValueOneofCase.Flags => UInt32Filter(span.Span.Flags, filter.Flags),
            WhereSpanPropertyFilter.ValueOneofCase.EventTimeUnixNano => span.Span.Events.Any(spanEvent => UInt64Filter(spanEvent.TimeUnixNano, filter.EventTimeUnixNano)),
            WhereSpanPropertyFilter.ValueOneofCase.EventName => span.Span.Events.Any(spanEvent => StringFilter(spanEvent.Name, filter.EventName)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkTraceId => span.Span.Links.Any(link => ByteStringFilter(link.TraceId, filter.LinkTraceId)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkSpanId => span.Span.Links.Any(link => ByteStringFilter(link.SpanId, filter.LinkSpanId)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkTraceState => span.Span.Links.Any(link => StringFilter(link.TraceState, filter.LinkTraceState)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkFlags => span.Span.Links.Any(link => UInt32Filter(link.Flags, filter.LinkFlags)),
            WhereSpanPropertyFilter.ValueOneofCase.LinkAttribute => span.Span.Links.Any(link => KeyValueFilter(link.Attributes, filter.LinkAttribute)),
            WhereSpanPropertyFilter.ValueOneofCase.EventAttribute => span.Span.Events.Any(spanEvent => KeyValueFilter(spanEvent.Attributes, filter.EventAttribute)),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeAttribute => KeyValueFilter(span.InstrumentationScope.Attributes, filter.InstrumentationScopeAttribute),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeName => StringFilter(span.InstrumentationScope.Name, filter.InstrumentationScopeName),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeSchemaUrl => StringFilter(span.InstrumentationScopeSchemaUrl, filter.InstrumentationScopeSchemaUrl),
            WhereSpanPropertyFilter.ValueOneofCase.InstrumentationScopeVersion => StringFilter(span.InstrumentationScope.Version, filter.InstrumentationScopeVersion),
            WhereSpanPropertyFilter.ValueOneofCase.ResourceAttribute => KeyValueFilter(span.Resource.Attributes, filter.ResourceAttribute),
            WhereSpanPropertyFilter.ValueOneofCase.ResourceSchemaUrl => StringFilter(span.ResourceSchemaUrl, filter.ResourceSchemaUrl),
            WhereSpanPropertyFilter.ValueOneofCase.None => false,
            _ => false
        };
    }

    private static bool StringFilter(string value, StringProperty property)
    {
        return property.CompareAs switch
        {
            StringCompareAsType.Equals => value.Equals(property.Compare, StringComparison.Ordinal),
            StringCompareAsType.NotEquals => !value.Equals(property.Compare, StringComparison.Ordinal),
            StringCompareAsType.Contains => value.Contains(property.Compare),
            StringCompareAsType.NotContains => !value.Contains(property.Compare),
            StringCompareAsType.IsEmpty => string.IsNullOrEmpty(value),
            StringCompareAsType.IsNotEmpty => !string.IsNullOrEmpty(value),
            StringCompareAsType.None => false,
            _ => false
        };
    }

    private static bool ByteStringFilter(ByteString value, ByteStringProperty property)
    {
        return property.CompareAs switch
        {
            ByteStringCompareAsType.Equals => value.Equals(property.Compare),
            ByteStringCompareAsType.NotEquals => !value.Equals(property.Compare),
            ByteStringCompareAsType.Empty => value.IsEmpty,
            ByteStringCompareAsType.NotEmpty => !value.IsEmpty,
            ByteStringCompareAsType.None => false,
            _ => false
        };
    }

    private static bool UInt64Filter(ulong value, UInt64Property property)
    {
        return property.CompareAs switch
        {
            NumberCompareAsType.Equals => value.Equals(property.Compare),
            NumberCompareAsType.NotEquals => !value.Equals(property.Compare),
            NumberCompareAsType.GreaterThanEquals => value >= property.Compare,
            NumberCompareAsType.GreaterThan => value > property.Compare,
            NumberCompareAsType.LessThanEquals => value <= property.Compare,
            NumberCompareAsType.LessThan => value < property.Compare,
            NumberCompareAsType.None => false,
            _ => false
        };
    }
    
    private static bool UInt32Filter(uint value, UInt32Property property)
    {
        return property.CompareAs switch
        {
            NumberCompareAsType.Equals => value.Equals(property.Compare),
            NumberCompareAsType.NotEquals => !value.Equals(property.Compare),
            NumberCompareAsType.GreaterThanEquals => value >= property.Compare,
            NumberCompareAsType.GreaterThan => value > property.Compare,
            NumberCompareAsType.LessThanEquals => value <= property.Compare,
            NumberCompareAsType.LessThan => value < property.Compare,
            NumberCompareAsType.None => false,
            _ => false
        };
    }
    
    private static bool Int64Filter(long value, Int64Property property)
    {
        return property.CompareAs switch
        {
            NumberCompareAsType.Equals => value.Equals(property.Compare),
            NumberCompareAsType.NotEquals => !value.Equals(property.Compare),
            NumberCompareAsType.GreaterThanEquals => value >= property.Compare,
            NumberCompareAsType.GreaterThan => value > property.Compare,
            NumberCompareAsType.LessThanEquals => value <= property.Compare,
            NumberCompareAsType.LessThan => value < property.Compare,
            NumberCompareAsType.None => false,
            _ => false
        };
    }
    
    private static bool DoubleFilter(double value, DoubleProperty property)
    {
        return property.CompareAs switch
        {
            NumberCompareAsType.Equals => value.Equals(property.Compare),
            NumberCompareAsType.NotEquals => !value.Equals(property.Compare),
            NumberCompareAsType.GreaterThanEquals => value >= property.Compare,
            NumberCompareAsType.GreaterThan => value > property.Compare,
            NumberCompareAsType.LessThanEquals => value <= property.Compare,
            NumberCompareAsType.LessThan => value < property.Compare,
            NumberCompareAsType.None => false,
            _ => false
        };
    }
    
    private static bool BoolFilter(bool value, BoolProperty property)
    {
        return property.CompareAs switch
        {
            BoolCompareAsType.Equals => value.Equals(property.Compare),
            BoolCompareAsType.NotEquals => !value.Equals(property.Compare),
            BoolCompareAsType.None => false,
            _ => false
        };
    }

    private static bool StatusCodeFilter(Status.Types.StatusCode value, SpanStatusCodeProperty property)
    {
        return property.CompareAs switch
        {
            EnumCompareAsType.Equals => value.Equals(property.Compare),
            EnumCompareAsType.NotEquals => !value.Equals(property.Compare),
            EnumCompareAsType.None => false,
            _ => false
        };
    }

    private static bool KindFilter(Span.Types.SpanKind value, SpanKindProperty property)
    {
        return property.CompareAs switch
        {
            EnumCompareAsType.Equals => value.Equals(property.Compare),
            EnumCompareAsType.NotEquals => !value.Equals(property.Compare),
            EnumCompareAsType.None => false,
            _ => false
        };
    }

    private static bool KeyValueFilter(RepeatedField<KeyValue> values, KeyValueProperty property)
    {
        var keyValue = values.FirstOrDefault(kv => kv.Key == property.Key);
        if (keyValue is not null)
        {
            // TODO add support for ArrayValue and KvListValue
            return property.ValueCase switch
            {
                KeyValueProperty.ValueOneofCase.None => false,
                KeyValueProperty.ValueOneofCase.StringValue => StringFilter(keyValue.Value.StringValue,
                    property.StringValue),
                KeyValueProperty.ValueOneofCase.ByteStringValue => ByteStringFilter(keyValue.Value.BytesValue, 
                    property.ByteStringValue),
                KeyValueProperty.ValueOneofCase.Int64Value => Int64Filter(keyValue.Value.IntValue, 
                    property.Int64Value),
                KeyValueProperty.ValueOneofCase.BoolValue => BoolFilter(keyValue.Value.BoolValue, 
                    property.BoolValue),
                KeyValueProperty.ValueOneofCase.DoubleValue => DoubleFilter(keyValue.Value.DoubleValue, 
                    property.DoubleValue),
                KeyValueProperty.ValueOneofCase.ArrayValue => ArrayFilter(keyValue.Value.ArrayValue.Values, property.ArrayValue),
            };
        }

        return false;
    }

    private static bool ArrayFilter(RepeatedField<AnyValue> values, ArrayProperty property)
    {
        return property.CompareAs switch
        {
            ArrayCompareAsType.Contains => ArrayContainsFilter(values, property),
            ArrayCompareAsType.DoesNotContain => !ArrayContainsFilter(values, property),
            ArrayCompareAsType.None => false,
            _ => false
        };
    }

    private static bool ArrayContainsFilter(RepeatedField<AnyValue> values, ArrayProperty property)
    {
        return property.Compare.ValueCase switch
        {
            AnyValue.ValueOneofCase.StringValue => values.Any(value => value.HasStringValue && StringFilter(value.StringValue, new StringProperty { CompareAs = StringCompareAsType.Equals, Compare = property.Compare.StringValue })),
            AnyValue.ValueOneofCase.BoolValue => values.Any(value => value.HasBoolValue && BoolFilter(value.BoolValue, new BoolProperty { CompareAs = BoolCompareAsType.Equals, Compare = property.Compare.BoolValue })),
            AnyValue.ValueOneofCase.IntValue => values.Any(value => value.HasIntValue && Int64Filter(value.IntValue, new Int64Property { CompareAs = NumberCompareAsType.Equals, Compare = property.Compare.IntValue })),
            AnyValue.ValueOneofCase.DoubleValue => values.Any(value => value.HasDoubleValue && DoubleFilter(value.DoubleValue, new DoubleProperty { CompareAs = NumberCompareAsType.Equals, Compare = property.Compare.DoubleValue })),
            AnyValue.ValueOneofCase.BytesValue => values.Any(value => value.HasBytesValue && ByteStringFilter(value.BytesValue, new ByteStringProperty { CompareAs = ByteStringCompareAsType.Equals, Compare = property.Compare.BytesValue })),
            AnyValue.ValueOneofCase.ArrayValue => property.Compare.ArrayValue.Values.All(compareValue =>
            {
                return values.Any(value =>
                {
                    return value.ValueCase switch
                    {
                        AnyValue.ValueOneofCase.StringValue => compareValue.HasStringValue && StringFilter(value.StringValue, new StringProperty { CompareAs = StringCompareAsType.Equals, Compare = compareValue.StringValue }),
                        AnyValue.ValueOneofCase.BoolValue => compareValue.HasBoolValue && BoolFilter(value.BoolValue, new BoolProperty { CompareAs = BoolCompareAsType.Equals, Compare = compareValue.BoolValue }),
                        AnyValue.ValueOneofCase.IntValue => compareValue.HasIntValue && Int64Filter(value.IntValue, new Int64Property { CompareAs = NumberCompareAsType.Equals, Compare = compareValue.IntValue }),
                        AnyValue.ValueOneofCase.DoubleValue => compareValue.HasDoubleValue && DoubleFilter(value.DoubleValue, new DoubleProperty { CompareAs = NumberCompareAsType.Equals, Compare = compareValue.DoubleValue }),
                        AnyValue.ValueOneofCase.BytesValue => compareValue.HasBytesValue && ByteStringFilter(value.BytesValue, new ByteStringProperty { CompareAs = ByteStringCompareAsType.Equals, Compare = compareValue.BytesValue}),
                        AnyValue.ValueOneofCase.ArrayValue => compareValue.ArrayValue.Values.Count > 0 && ArrayContainsFilter(values, new ArrayProperty { CompareAs = ArrayCompareAsType.Contains, Compare = new AnyValue { ArrayValue = compareValue.ArrayValue }}),
                        AnyValue.ValueOneofCase.None => false,
                    };
                });
            }),
            AnyValue.ValueOneofCase.None => false,
        };
    }
}