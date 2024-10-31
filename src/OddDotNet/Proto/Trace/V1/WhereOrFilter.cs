namespace OddDotNet.Proto.Trace.V1;

public sealed partial class WhereOrFilter : IWhere<FlatSpan>
{
    public bool Matches(FlatSpan signal) => Filters.Any(whereFilter => whereFilter.Matches(signal));
}