using OddDotNet.Proto.AppInsights.V1;
using OddDotNet.Proto.Common.V1;

namespace OddDotNet.Filters.AppInsights;

public static class SeverityLevelFilter
{
    public static bool Matches(SeverityLevel value, SeverityLevelProperty property) => property.CompareAs switch
    {
        EnumCompareAsType.Equals => value == property.Compare,
        EnumCompareAsType.NotEquals => value != property.Compare,
        EnumCompareAsType.NoneUnspecified => false,
        _ => false
    };
}
