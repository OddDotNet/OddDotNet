using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class HistogramDataPointFilter : IWhere<HistogramDataPoint>
{
    public bool Matches(HistogramDataPoint signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Attributes => KeyValueListFilter.Matches(signal.Attributes, Attributes),
        ValueOneofCase.StartTimeUnixNano => UInt64Filter.Matches(signal.StartTimeUnixNano, StartTimeUnixNano),
        ValueOneofCase.TimeUnixNano => UInt64Filter.Matches(signal.TimeUnixNano, TimeUnixNano),
        ValueOneofCase.Count => UInt64Filter.Matches(signal.Count, Count),
        ValueOneofCase.Sum => DoubleFilter.Matches(signal.Sum, Sum),
        ValueOneofCase.BucketCount => signal.BucketCounts.Any(bucketCount => UInt64Filter.Matches(bucketCount, BucketCount)),
        ValueOneofCase.ExplicitBound => signal.ExplicitBounds.Any(bound => DoubleFilter.Matches(bound, ExplicitBound)),
        ValueOneofCase.Exemplar => signal.Exemplars.Any(exemplar => Exemplar.Matches(exemplar)),
        ValueOneofCase.Flags => UInt32Filter.Matches(signal.Flags, Flags),
        ValueOneofCase.Min => DoubleFilter.Matches(signal.Min, Min),
        ValueOneofCase.Max => DoubleFilter.Matches(signal.Max, Max),
        _ => false
    };
}