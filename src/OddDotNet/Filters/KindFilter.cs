using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Filters;

public static class KindFilter
{
    public static bool Matches(Span.Types.SpanKind value, SpanKindProperty property) => property.CompareAs switch
    {
        EnumCompareAsType.Equals => value.Equals(property.Compare),
        EnumCompareAsType.NotEquals => !value.Equals(property.Compare),
        EnumCompareAsType.NoneUnspecified => false,
        _ => false
    };
}