using OddDotNet.Proto.Common.V1;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class AnyValueFilter
{
    public static bool Matches(AnyValue value, AnyValueProperty property) => property.ValueCase switch
    {
        AnyValueProperty.ValueOneofCase.None => false,
        AnyValueProperty.ValueOneofCase.StringValue => value.HasStringValue && StringFilter.Matches(value.StringValue, property.StringValue),
        AnyValueProperty.ValueOneofCase.ByteStringValue => value.HasBytesValue && ByteStringFilter.Matches(value.BytesValue, property.ByteStringValue),
        AnyValueProperty.ValueOneofCase.Int64Value => value.HasIntValue && Int64Filter.Matches(value.IntValue, property.Int64Value),
        AnyValueProperty.ValueOneofCase.BoolValue => value.HasBoolValue && BoolFilter.Matches(value.BoolValue, property.BoolValue),
        AnyValueProperty.ValueOneofCase.DoubleValue => value.HasDoubleValue && DoubleFilter.Matches(value.DoubleValue, property.DoubleValue),
        AnyValueProperty.ValueOneofCase.ArrayValue => value.ArrayValue is not null && value.ArrayValue.Values.Any(item => Matches(item, property.ArrayValue)),
        AnyValueProperty.ValueOneofCase.KeyValue => value.KvlistValue is not null && KeyValueFilter.Matches(value.KvlistValue.Values, property.KeyValue),
        _ => false
    };
}