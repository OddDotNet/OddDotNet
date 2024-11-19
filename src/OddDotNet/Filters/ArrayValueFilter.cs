using OddDotNet.Proto.Common.V1;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class ArrayValueFilter
{
    public static bool Matches(ArrayValue array, ArrayValueProperty property) =>
        property.Values.All(prop => array.Values.Any(item => AnyValueFilter.Matches(item, prop)));
}