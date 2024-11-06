using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class SummaryDataPointFilter : IWhere<SummaryDataPoint>
{
    public bool Matches(SummaryDataPoint signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Attribute => KeyValueFilter.Matches(signal.Attributes, Attribute),
        ValueOneofCase.StartTimeUnixNano => UInt64Filter.Matches(signal.StartTimeUnixNano, StartTimeUnixNano),
        ValueOneofCase.TimeUnixNano => UInt64Filter.Matches(signal.TimeUnixNano, TimeUnixNano),
        ValueOneofCase.Count => UInt64Filter.Matches(signal.Count, Count),
        ValueOneofCase.Sum => DoubleFilter.Matches(signal.Sum, Sum),
        ValueOneofCase.QuantileValue => signal.QuantileValues.Any(valueAtQuantile => QuantileValue.Matches(valueAtQuantile)),
        ValueOneofCase.Flags => UInt32Filter.Matches(signal.Flags, Flags),
        _ => false
    };
}