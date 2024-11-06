using Google.Protobuf.Collections;
using OddDotNet.Proto.Common.V1;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class KeyValueFilter
{
    public static bool Matches(RepeatedField<KeyValue> map, KeyValueProperty property)
    {
        var keyValue = map.FirstOrDefault(kv => kv.Key == property.Key);
        if (keyValue is not null)
        {
            // TODO add support for ArrayValue and KvListValue
            return property.ValueCase switch
            {
                KeyValueProperty.ValueOneofCase.StringValue => StringFilter.Matches(keyValue.Value.StringValue,
                    property.StringValue),
                KeyValueProperty.ValueOneofCase.ByteStringValue => ByteStringFilter.Matches(keyValue.Value.BytesValue, 
                    property.ByteStringValue),
                KeyValueProperty.ValueOneofCase.Int64Value => Int64Filter.Matches(keyValue.Value.IntValue, 
                    property.Int64Value),
                KeyValueProperty.ValueOneofCase.BoolValue => BoolFilter.Matches(keyValue.Value.BoolValue, 
                    property.BoolValue),
                KeyValueProperty.ValueOneofCase.DoubleValue => DoubleFilter.Matches(keyValue.Value.DoubleValue, 
                    property.DoubleValue),
                KeyValueProperty.ValueOneofCase.None => false,
                _ => false
            };
        }

        return false;
    }
}