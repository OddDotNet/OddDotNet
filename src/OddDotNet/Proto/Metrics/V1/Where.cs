namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class Where : IWhere<FlatMetric>
{
    public bool Matches(FlatMetric signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal),
        ValueOneofCase.Gauge => Gauge.Matches(signal.Metric.Gauge),
        ValueOneofCase.Sum => Sum.Matches(signal.Metric.Sum),
        ValueOneofCase.Histogram => Histogram.Matches(signal.Metric.Histogram),
        ValueOneofCase.ExponentialHistogram => ExponentialHistogram.Matches(signal.Metric.ExponentialHistogram),
        ValueOneofCase.Summary => Summary.Matches(signal.Metric.Summary),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.InstrumentationScope => InstrumentationScope.Matches(signal.InstrumentationScope),
        ValueOneofCase.Resource => Resource.Matches(signal.Resource)
    };
}