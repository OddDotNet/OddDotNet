using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class ExponentialHistogramDataPointFilter : IWhere<ExponentialHistogramDataPoint>
{
    public bool Matches(ExponentialHistogramDataPoint signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Attribute => KeyValueFilter.Matches(signal.Attributes, Attribute),
        ValueOneofCase.StartTimeUnixNano => UInt64Filter.Matches(signal.StartTimeUnixNano, StartTimeUnixNano),
        ValueOneofCase.TimeUnixNano => UInt64Filter.Matches(signal.TimeUnixNano, TimeUnixNano),
        ValueOneofCase.Count => UInt64Filter.Matches(signal.Count, Count),
        ValueOneofCase.Sum => DoubleFilter.Matches(signal.Sum, Sum),
        ValueOneofCase.Scale => Int32Filter.Matches(signal.Scale, Scale),
        ValueOneofCase.ZeroCount => UInt64Filter.Matches(signal.ZeroCount, ZeroCount),
        ValueOneofCase.Positive => Positive.Matches(signal.Positive),
        ValueOneofCase.Negative => Negative.Matches(signal.Negative),
        ValueOneofCase.Flags => UInt32Filter.Matches(signal.Flags, Flags),
        ValueOneofCase.Exemplar => signal.Exemplars.Any(exemplar => Exemplar.Matches(exemplar)),
        ValueOneofCase.Min => DoubleFilter.Matches(signal.Min, Min),
        ValueOneofCase.Max => DoubleFilter.Matches(signal.Max, Max),
        ValueOneofCase.ZeroThreshold => DoubleFilter.Matches(signal.ZeroThreshold, ZeroThreshold),
        _ => false
    };
}