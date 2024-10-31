namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class Where : IWhere<FlatMetric>
{
    public bool Matches(FlatMetric signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal),
        ValueOneofCase.Gauge => Gauge.Matches(signal.Metric.Gauge),
        
    };
}