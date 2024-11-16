using OddDotNet.Proto.Common.V1;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class AnyValueFilter
{
    public static bool Matches(AnyValue value, AnyValueProperty property) => property.ValueCase switch
    {
        AnyValueProperty.ValueOneofCase.None => false,
        AnyValueProperty.ValueOneofCase.StringValue => StringFilter.Matches(value.StringValue, property.StringValue),
        AnyValueProperty.ValueOneofCase.ByteStringValue => ByteStringFilter.Matches(value.BytesValue, property.ByteStringValue),
        AnyValueProperty.ValueOneofCase.Int64Value => Int64Filter.Matches(value.IntValue, property.Int64Value),
        AnyValueProperty.ValueOneofCase.BoolValue => BoolFilter.Matches(value.BoolValue, property.BoolValue),
        AnyValueProperty.ValueOneofCase.DoubleValue => DoubleFilter.Matches(value.DoubleValue, property.DoubleValue),
    };
}