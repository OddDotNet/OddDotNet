using OddDotNet.Proto.AppInsights.V1;
using Google.Protobuf.Collections;

namespace OddDotNet.Filters.AppInsights;

public static class PropertyMapFilter
{
    public static bool Matches(MapField<string, string> properties, PropertyMapProperty filter)
    {
        if (!properties.TryGetValue(filter.Key, out var value))
            return false;

        return StringFilter.Matches(value, filter.Value);
    }
}
