using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Metrics.V1;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Filters;

public static class AggregationTemporalityFilter
{
    public static bool Matches(AggregationTemporality value, AggregationTemporalityProperty property) =>
        property.CompareAs switch
        {
            EnumCompareAsType.None => false,
            EnumCompareAsType.Equals => value.Equals(property.Compare),
            EnumCompareAsType.NotEquals => !value.Equals(property.Compare),
            _ => false
        };
}