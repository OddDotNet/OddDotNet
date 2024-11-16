using OddDotNet.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class BoolFilter
{
    public static bool Matches(bool value, BoolProperty property) => property.CompareAs switch
    {
        BoolCompareAsType.Equals => value.Equals(property.Compare),
        BoolCompareAsType.NotEquals => !value.Equals(property.Compare),
        BoolCompareAsType.NoneUnspecified => false,
        _ => false
    };
}