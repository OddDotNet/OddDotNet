namespace OddDotNet.Proto.Logs.V1;

public sealed partial class OrFilter : IWhere<FlatLog>
{
    public bool Matches(FlatLog signal) => Filters.Any(whereFilter => whereFilter.Matches(signal));
}