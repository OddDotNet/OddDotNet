using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class SummaryFilter : IWhere<Summary>
{
    public bool Matches(Summary signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.DataPoint => signal.DataPoints.Any(dataPoint => DataPoint.Matches(dataPoint)),
        _ => false
    };
}