using OddDotNet.Filters;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Proto.Trace.V1;

public sealed partial class LinkFilter : IWhere<Span.Types.Link>
{
    public bool Matches(Span.Types.Link signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.TraceId => ByteStringFilter.Matches(signal.TraceId, TraceId),
        ValueOneofCase.SpanId => ByteStringFilter.Matches(signal.SpanId, SpanId),
        ValueOneofCase.TraceState => StringFilter.Matches(signal.TraceState, TraceState),
        ValueOneofCase.Attributes => KeyValueListFilter.Matches(signal.Attributes, Attributes),
        ValueOneofCase.DroppedAttributesCount => UInt32Filter.Matches(signal.DroppedAttributesCount, DroppedAttributesCount),
        ValueOneofCase.Flags => UInt32Filter.Matches(signal.Flags, Flags),
        _ => false
    };
}