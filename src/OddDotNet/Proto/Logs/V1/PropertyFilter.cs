using OddDotNet.Filters;
using OpenTelemetry.Proto.Logs.V1;

namespace OddDotNet.Proto.Logs.V1;

public sealed partial class PropertyFilter : IWhere<LogRecord>
{
    public bool Matches(LogRecord signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.TimeUnixNano => UInt64Filter.Matches(signal.TimeUnixNano, TimeUnixNano),
        ValueOneofCase.ObservedTimeUnixNano => UInt64Filter.Matches(signal.ObservedTimeUnixNano, ObservedTimeUnixNano),
        ValueOneofCase.SeverityNumber => SeverityNumberFilter.Matches(signal.SeverityNumber, SeverityNumber),
        ValueOneofCase.SeverityText => StringFilter.Matches(signal.SeverityText, SeverityText),
        ValueOneofCase.Body => AnyValueFilter.Matches(signal.Body, Body),
        ValueOneofCase.Attributes => KeyValueListFilter.Matches(signal.Attributes, Attributes),
        ValueOneofCase.DroppedAttributesCount => UInt32Filter.Matches(signal.DroppedAttributesCount, DroppedAttributesCount),
        ValueOneofCase.Flags => UInt32Filter.Matches(signal.Flags, Flags),
        ValueOneofCase.TraceId => ByteStringFilter.Matches(signal.TraceId, TraceId),
        ValueOneofCase.SpanId => ByteStringFilter.Matches(signal.SpanId, SpanId),
        _ => false
    };
}