using OddDotNet.Proto.AppInsights.V1;
using Google.Protobuf.Collections;

namespace OddDotNet.Filters.AppInsights;

public static class MeasurementMapFilter
{
    public static bool Matches(MapField<string, double> measurements, MeasurementMapProperty filter)
    {
        if (!measurements.TryGetValue(filter.Key, out var value))
            return false;

        return DoubleFilter.Matches(value, filter.Value);
    }
}
