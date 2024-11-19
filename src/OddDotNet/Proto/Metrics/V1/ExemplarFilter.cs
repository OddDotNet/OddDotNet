using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class ExemplarFilter : IWhere<Exemplar>
{
    public bool Matches(Exemplar signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.FilteredAttributes => KeyValueListFilter.Matches(signal.FilteredAttributes, FilteredAttributes),
        ValueOneofCase.TimeUnixNano => UInt64Filter.Matches(signal.TimeUnixNano, TimeUnixNano),
        ValueOneofCase.ValueAsDouble => DoubleFilter.Matches(signal.AsDouble, ValueAsDouble),
        ValueOneofCase.ValueAsInt => Int64Filter.Matches(signal.AsInt, ValueAsInt),
        ValueOneofCase.SpanId => ByteStringFilter.Matches(signal.SpanId, SpanId),
        ValueOneofCase.TraceId => ByteStringFilter.Matches(signal.TraceId, TraceId),
        _ => false
    };
}