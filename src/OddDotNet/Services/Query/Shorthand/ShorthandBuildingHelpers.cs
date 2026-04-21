using Google.Protobuf;

using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.Common.V1;

namespace OddDotNet.Services.Query.Shorthand;

internal static class ShorthandBuildingHelpers
{
    public static StringProperty Eq(string value) =>
        new() { CompareAs = StringCompareAsType.Equals, Compare = value };

    public static ByteStringProperty EqBytes(string paramName, string hexValue)
    {
        try
        {
            var bytes = Convert.FromHexString(hexValue);
            return new ByteStringProperty
            {
                CompareAs = ByteStringCompareAsType.Equals,
                Compare = ByteString.CopyFrom(bytes)
            };
        }
        catch (FormatException)
        {
            throw new ShorthandParseException($"'{paramName}' must be hex-encoded; got '{hexValue}'");
        }
    }

    public static KeyValueProperty BuildAttr(string key, string value) => new()
    {
        Key = key,
        Value = new AnyValueProperty { StringValue = Eq(value) }
    };

    public static void RejectUnknown(string paramName, string signal) =>
        throw new ShorthandParseException(
            $"'{paramName}' is not a recognized field or attr filter for signal '{signal}'");
}
