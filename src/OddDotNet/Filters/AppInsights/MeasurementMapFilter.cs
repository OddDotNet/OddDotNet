using OddDotNet.Proto.AppInsights.V1;
using Google.Protobuf.Collections;

namespace OddDotNet.Filters.AppInsights;

public static class MeasurementMapFilterHelper
{
    public static bool Matches(MapField<string, double> measurements, MeasurementMapFilter filter)
    {
        if (!measurements.TryGetValue(filter.Key, out var value))
            return false;
        
        return DoubleFilter.Matches(value, filter.Value);
    }
}
