using OddDotNet.Filters;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Proto.Trace.V1;

public sealed partial class EventFilter : IWhere<Span.Types.Event>
{
    public bool Matches(Span.Types.Event signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.TimeUnixNano => UInt64Filter.Matches(signal.TimeUnixNano, TimeUnixNano),
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Attributes => KeyValueListFilter.Matches(signal.Attributes, Attributes),
        ValueOneofCase.DroppedAttributesCount => UInt32Filter.Matches(signal.DroppedAttributesCount, DroppedAttributesCount),
        _ => false
    };
}