using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class ExponentialHistogramFilter : IWhere<ExponentialHistogram>
{
    public bool Matches(ExponentialHistogram signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.DataPoint => signal.DataPoints.Any(dataPoint => DataPoint.Matches(dataPoint)),
        ValueOneofCase.AggregationTemporality => AggregationTemporalityFilter.Matches(signal.AggregationTemporality, AggregationTemporality),
        _ => false
    };
}