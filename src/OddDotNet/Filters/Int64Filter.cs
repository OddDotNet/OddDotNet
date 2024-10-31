using OddDotNet.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class Int64Filter
{
    public static bool Matches(long value, Int64Property property) => property.CompareAs switch
    {
        NumberCompareAsType.Equals => value.Equals(property.Compare),
        NumberCompareAsType.NotEquals => !value.Equals(property.Compare),
        NumberCompareAsType.GreaterThanEquals => value >= property.Compare,
        NumberCompareAsType.GreaterThan => value > property.Compare,
        NumberCompareAsType.LessThanEquals => value <= property.Compare,
        NumberCompareAsType.LessThan => value < property.Compare,
        NumberCompareAsType.None => false,
        _ => false
    };
}