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
        AnyValueProperty.ValueOneofCase.IntValue => value.HasIntValue && Int64Filter.Matches(value.IntValue, property.IntValue),
        AnyValueProperty.ValueOneofCase.BoolValue => value.HasBoolValue && BoolFilter.Matches(value.BoolValue, property.BoolValue),
        AnyValueProperty.ValueOneofCase.DoubleValue => value.HasDoubleValue && DoubleFilter.Matches(value.DoubleValue, property.DoubleValue),
        AnyValueProperty.ValueOneofCase.ArrayValue => value.ArrayValue is not null && ArrayValueFilter.Matches(value.ArrayValue, property.ArrayValue),
        AnyValueProperty.ValueOneofCase.KvlistValue => value.KvlistValue is not null && KeyValueListFilter.Matches(value.KvlistValue, property.KvlistValue),
        _ => false
    };
}