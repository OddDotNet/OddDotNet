using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class ValueAtQuantileFilter : IWhere<SummaryDataPoint.Types.ValueAtQuantile>
{
    public bool Matches(SummaryDataPoint.Types.ValueAtQuantile signal) => PropertyCase switch
    {
        PropertyOneofCase.None => false,
        PropertyOneofCase.Quantile => DoubleFilter.Matches(signal.Quantile, Quantile),
        PropertyOneofCase.Value => DoubleFilter.Matches(signal.Value, Value),
        _ => false
    };
}