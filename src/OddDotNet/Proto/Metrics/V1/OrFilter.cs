namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class OrFilter : IWhere<FlatMetric>
{
    public bool Matches(FlatMetric signal) => Filters.Any(filter => filter.Matches(signal));
}