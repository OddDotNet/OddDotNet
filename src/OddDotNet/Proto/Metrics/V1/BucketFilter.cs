using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class BucketFilter : IWhere<ExponentialHistogramDataPoint.Types.Buckets>
{
    public bool Matches(ExponentialHistogramDataPoint.Types.Buckets signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Offset => Int32Filter.Matches(signal.Offset, Offset),
        ValueOneofCase.BucketCount => signal.BucketCounts.Any(count => UInt64Filter.Matches(count, BucketCount)),
        _ => false
    };
}