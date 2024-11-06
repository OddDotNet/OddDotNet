using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Filters;

public static class StatusCodeFilter
{
    public static bool Matches(Status.Types.StatusCode value, SpanStatusCodeProperty property) => property.CompareAs switch
    {
        EnumCompareAsType.Equals => value.Equals(property.Compare),
        EnumCompareAsType.NotEquals => !value.Equals(property.Compare),
        EnumCompareAsType.None => false,
        _ => false
    };
}