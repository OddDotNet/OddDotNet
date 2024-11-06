using OddDotNet.Filters;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Proto.Trace.V1;

public sealed partial class PropertyFilter : IWhere<Span>
{
    public bool Matches(Span signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.TraceId => ByteStringFilter.Matches(signal.TraceId, TraceId),
        ValueOneofCase.SpanId => ByteStringFilter.Matches(signal.SpanId, SpanId),
        ValueOneofCase.TraceState => StringFilter.Matches(signal.TraceState, TraceState),
        ValueOneofCase.ParentSpanId => ByteStringFilter.Matches(signal.ParentSpanId, ParentSpanId),
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Kind => KindFilter.Matches(signal.Kind, Kind),
        ValueOneofCase.StartTimeUnixNano => UInt64Filter.Matches(signal.StartTimeUnixNano, StartTimeUnixNano),
        ValueOneofCase.EndTimeUnixNano => UInt64Filter.Matches(signal.EndTimeUnixNano, EndTimeUnixNano),
        ValueOneofCase.Attribute => KeyValueFilter.Matches(signal.Attributes, Attribute),
        ValueOneofCase.DroppedAttributesCount => UInt32Filter.Matches(signal.DroppedAttributesCount, DroppedAttributesCount),
        ValueOneofCase.Event => signal.Events.Any(evt => Event.Matches(evt)),
        ValueOneofCase.DroppedEventsCount => UInt32Filter.Matches(signal.DroppedEventsCount, DroppedEventsCount),
        ValueOneofCase.Link => signal.Links.Any(link => Link.Matches(link)),
        ValueOneofCase.DroppedLinksCount => UInt32Filter.Matches(signal.DroppedLinksCount, DroppedLinksCount),
        ValueOneofCase.Status => Status.Matches(signal.Status),
        ValueOneofCase.Flags => UInt32Filter.Matches(signal.Flags, Flags),
        _ => false
    };
}