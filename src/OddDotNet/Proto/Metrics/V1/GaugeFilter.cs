using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class GaugeFilter : IWhere<Gauge>
{
    public bool Matches(Gauge signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.DataPoint => signal.DataPoints.Any(dataPoint => DataPoint.Matches(dataPoint))
    };
}