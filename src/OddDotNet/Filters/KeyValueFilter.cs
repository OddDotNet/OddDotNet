using Google.Protobuf.Collections;
using OddDotNet.Proto.Common.V1;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class KeyValueFilter
{
    public static bool Matches(KeyValue kv, KeyValueProperty property)
    {
        bool isMatch = false;
        if (kv.Key == property.Key)
        {
            isMatch = property.Value.ValueCase switch
            {
                AnyValueProperty.ValueOneofCase.None => false,
                AnyValueProperty.ValueOneofCase.StringValue => kv.Value.HasStringValue && StringFilter.Matches(kv.Value.StringValue, property.Value.StringValue),
                AnyValueProperty.ValueOneofCase.BoolValue => kv.Value.HasBoolValue && BoolFilter.Matches(kv.Value.BoolValue, property.Value.BoolValue),
                AnyValueProperty.ValueOneofCase.IntValue => kv.Value.HasIntValue && Int64Filter.Matches(kv.Value.IntValue, property.Value.IntValue),
                AnyValueProperty.ValueOneofCase.DoubleValue => kv.Value.HasDoubleValue && DoubleFilter.Matches(kv.Value.DoubleValue, property.Value.DoubleValue),
                AnyValueProperty.ValueOneofCase.ArrayValue => kv.Value.ArrayValue is not null && ArrayValueFilter.Matches(kv.Value.ArrayValue, property.Value.ArrayValue),
                AnyValueProperty.ValueOneofCase.KvlistValue => kv.Value.KvlistValue is not null && KeyValueListFilter.Matches(kv.Value.KvlistValue, property.Value.KvlistValue),
                AnyValueProperty.ValueOneofCase.ByteStringValue => kv.Value.HasBytesValue && ByteStringFilter.Matches(kv.Value.BytesValue, property.Value.ByteStringValue),
                _ => false
            };
        }
        
        return isMatch;
    }
}