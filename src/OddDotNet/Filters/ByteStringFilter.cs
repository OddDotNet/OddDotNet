using Google.Protobuf;
using OddDotNet.Proto.Common.V1;

namespace OddDotNet.Filters;

public static class ByteStringFilter
{
    public static bool Matches(ByteString value, ByteStringProperty property) => property.CompareAs switch
    {
        ByteStringCompareAsType.Equals => value.Equals(property.Compare),
        ByteStringCompareAsType.NotEquals => !value.Equals(property.Compare),
        ByteStringCompareAsType.Empty => value.IsEmpty,
        ByteStringCompareAsType.NotEmpty => !value.IsEmpty,
        ByteStringCompareAsType.NoneUnspecified => false,
        _ => false
    };
}