using Google.Protobuf.Collections;
using OddDotNet.Proto.Common.V1;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class KeyValueListFilter
{
    public static bool Matches(KeyValueList list, KeyValueListProperty property) => Matches(list.Values, property);

    public static bool Matches(RepeatedField<KeyValue> list, KeyValueListProperty property) =>
        property.Values.All(prop => list.Any(kvp => KeyValueFilter.Matches(kvp, prop)));
}