using OddDotNet.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class StringFilter
{
    public static bool Matches(string value, StringProperty property)
    {
        return property.CompareAs switch
        {
            StringCompareAsType.Equals => value.Equals(property.Compare, StringComparison.Ordinal),
            StringCompareAsType.NotEquals => !value.Equals(property.Compare, StringComparison.Ordinal),
            StringCompareAsType.Contains => value.Contains(property.Compare),
            StringCompareAsType.NotContains => !value.Contains(property.Compare),
            StringCompareAsType.IsEmpty => string.IsNullOrEmpty(value),
            StringCompareAsType.IsNotEmpty => !string.IsNullOrEmpty(value),
            StringCompareAsType.None => false,
            _ => false
        };
    }
}