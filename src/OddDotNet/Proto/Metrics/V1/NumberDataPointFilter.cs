using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class NumberDataPointFilter : IWhere<NumberDataPoint>
{
    public bool Matches(NumberDataPoint signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Flags => UInt32Filter.Matches(signal.Flags, Flags),
        ValueOneofCase.Attributes => KeyValueListFilter.Matches(signal.Attributes, Attributes),
        ValueOneofCase.StartTimeUnixNano => UInt64Filter.Matches(signal.StartTimeUnixNano, StartTimeUnixNano),
        ValueOneofCase.TimeUnixNano => UInt64Filter.Matches(signal.TimeUnixNano, TimeUnixNano),
        ValueOneofCase.ValueAsDouble => DoubleFilter.Matches(signal.AsDouble, ValueAsDouble),
        ValueOneofCase.ValueAsInt => Int64Filter.Matches(signal.AsInt, ValueAsInt),
        ValueOneofCase.Exemplar => signal.Exemplars.Any(exemplar => Exemplar.Matches(exemplar)),
        _ => false
    };
}