using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class HistogramFilter : IWhere<Histogram>
{
    public bool Matches(Histogram signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.DataPoint => signal.DataPoints.Any(dataPoint => DataPoint.Matches(dataPoint)),
        ValueOneofCase.AggregationTemporality => AggregationTemporalityFilter.Matches(signal.AggregationTemporality, AggregationTemporality),
        _ => false
    };
}