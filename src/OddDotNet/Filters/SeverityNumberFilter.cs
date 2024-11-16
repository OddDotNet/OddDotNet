using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Logs.V1;
using OpenTelemetry.Proto.Logs.V1;

namespace OddDotNet.Filters;

public static class SeverityNumberFilter
{
    public static bool Matches(SeverityNumber value, SeverityNumberProperty property) => property.CompareAs switch
    {
        EnumCompareAsType.Equals => value.Equals(property.Compare),
        EnumCompareAsType.NotEquals => !value.Equals(property.Compare),
        EnumCompareAsType.NoneUnspecified => false,
        _ => false
    };
}