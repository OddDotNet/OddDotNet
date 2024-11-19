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
            return property.Value.ValueCase switch
            {
                AnyValueProperty.ValueOneofCase.StringValue => keyValue.Value.HasStringValue && StringFilter.Matches(keyValue.Value.StringValue,
                    property.Value.StringValue),
                AnyValueProperty.ValueOneofCase.ByteStringValue => keyValue.Value.HasBytesValue && ByteStringFilter.Matches(keyValue.Value.BytesValue, 
                    property.Value.ByteStringValue),
                AnyValueProperty.ValueOneofCase.Int64Value => keyValue.Value.HasIntValue && Int64Filter.Matches(keyValue.Value.IntValue, 
                    property.Value.Int64Value),
                AnyValueProperty.ValueOneofCase.BoolValue => keyValue.Value.HasBoolValue && BoolFilter.Matches(keyValue.Value.BoolValue, 
                    property.Value.BoolValue),
                AnyValueProperty.ValueOneofCase.DoubleValue => keyValue.Value.HasDoubleValue && DoubleFilter.Matches(keyValue.Value.DoubleValue, 
                    property.Value.DoubleValue),
                AnyValueProperty.ValueOneofCase.ArrayValue => keyValue.Value.ArrayValue is not null && keyValue.Value.ArrayValue.Values.Any(value => AnyValueFilter.Matches(value, property.Value.ArrayValue)),
                AnyValueProperty.ValueOneofCase.KeyValue => keyValue.Value.KvlistValue is not null && Matches(keyValue.Value.KvlistValue.Values, property.Value.KeyValue),
                AnyValueProperty.ValueOneofCase.None => false,
                // _ => false
            };
        }

        return false;
    }
}