using OddDotNet.Filters;

namespace OddDotNet.Proto.Trace.V1;

public sealed partial class WherePropertyFilter : IWhere<FlatSpan>
{
    public bool Matches(FlatSpan signal) => ValueCase switch
    {
        ValueOneofCase.Name => StringFilter.Matches(signal.Span.Name, Name),
        ValueOneofCase.TraceState => StringFilter.Matches(signal.Span.TraceState, TraceState),
        ValueOneofCase.SpanId => ByteStringFilter.Matches(signal.Span.SpanId, SpanId),
        ValueOneofCase.TraceId => ByteStringFilter.Matches(signal.Span.TraceId, TraceId),
        ValueOneofCase.ParentSpanId => ByteStringFilter.Matches(signal.Span.ParentSpanId, ParentSpanId),
        ValueOneofCase.StartTimeUnixNano => UInt64Filter.Matches(signal.Span.StartTimeUnixNano, StartTimeUnixNano),
        ValueOneofCase.EndTimeUnixNano => UInt64Filter.Matches(signal.Span.EndTimeUnixNano, EndTimeUnixNano),
        ValueOneofCase.StatusCode => StatusCodeFilter.Matches(signal.Span.Status.Code, StatusCode),
        ValueOneofCase.Kind => KindFilter.Matches(signal.Span.Kind, Kind),
        ValueOneofCase.Attribute => KeyValueFilter.Matches(signal.Span.Attributes, Attribute),
        ValueOneofCase.Flags => UInt32Filter.Matches(signal.Span.Flags, Flags),
        ValueOneofCase.EventTimeUnixNano => signal.Span.Events.Any(spanEvent => UInt64Filter.Matches(spanEvent.TimeUnixNano, EventTimeUnixNano)),
        ValueOneofCase.EventName => signal.Span.Events.Any(spanEvent => StringFilter.Matches(spanEvent.Name, EventName)),
        ValueOneofCase.LinkTraceId => signal.Span.Links.Any(link => ByteStringFilter.Matches(link.TraceId, LinkTraceId)),
        ValueOneofCase.LinkSpanId => signal.Span.Links.Any(link => ByteStringFilter.Matches(link.SpanId, LinkSpanId)),
        ValueOneofCase.LinkTraceState => signal.Span.Links.Any(link => StringFilter.Matches(link.TraceState, LinkTraceState)),
        ValueOneofCase.LinkFlags => signal.Span.Links.Any(link => UInt32Filter.Matches(link.Flags, LinkFlags)),
        ValueOneofCase.LinkAttribute => signal.Span.Links.Any(link => KeyValueFilter.Matches(link.Attributes, LinkAttribute)),
        ValueOneofCase.EventAttribute => signal.Span.Events.Any(spanEvent => KeyValueFilter.Matches(spanEvent.Attributes, EventAttribute)),
        ValueOneofCase.InstrumentationScopeAttribute => KeyValueFilter.Matches(signal.InstrumentationScope.Attributes, InstrumentationScopeAttribute),
        ValueOneofCase.InstrumentationScopeName => StringFilter.Matches(signal.InstrumentationScope.Name, InstrumentationScopeName),
        ValueOneofCase.InstrumentationScopeSchemaUrl => StringFilter.Matches(signal.InstrumentationScopeSchemaUrl, InstrumentationScopeSchemaUrl),
        ValueOneofCase.InstrumentationScopeVersion => StringFilter.Matches(signal.InstrumentationScope.Version, InstrumentationScopeVersion),
        ValueOneofCase.ResourceAttribute => KeyValueFilter.Matches(signal.Resource.Attributes, ResourceAttribute),
        ValueOneofCase.ResourceSchemaUrl => StringFilter.Matches(signal.ResourceSchemaUrl, ResourceSchemaUrl),
        ValueOneofCase.None => false,
        _ => false
    };
}