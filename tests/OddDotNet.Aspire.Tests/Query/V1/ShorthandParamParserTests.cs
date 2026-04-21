using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.Query.Shorthand;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class ShorthandParamParserTests
{
    private static IQueryCollection Q(params (string k, string v)[] pairs)
    {
        var dict = pairs.ToDictionary(p => p.k, p => new StringValues(p.v));
        return new QueryCollection(dict);
    }

    [Fact]
    public void ParseTake_WhenMissing_ShouldReturnTakeFirst()
    {
        var take = ShorthandParamParser.ParseTake(Q());
        Assert.Equal(Take.ValueOneofCase.TakeFirst, take.ValueCase);
    }

    [Fact]
    public void ParseTake_WhenAll_ShouldReturnTakeAll()
    {
        Assert.Equal(Take.ValueOneofCase.TakeAll, ShorthandParamParser.ParseTake(Q(("take", "all"))).ValueCase);
    }

    [Fact]
    public void ParseTake_WhenFirst_ShouldReturnTakeFirst()
    {
        Assert.Equal(Take.ValueOneofCase.TakeFirst, ShorthandParamParser.ParseTake(Q(("take", "first"))).ValueCase);
    }

    [Theory]
    [InlineData("3", 3u)]
    [InlineData("0", 0u)]
    public void ParseTake_WhenInteger_ShouldReturnTakeExact(string input, uint expected)
    {
        var take = ShorthandParamParser.ParseTake(Q(("take", input)));
        Assert.Equal(Take.ValueOneofCase.TakeExact, take.ValueCase);
        Assert.Equal(expected, (uint)take.TakeExact.Count);
    }

    [Fact]
    public void ParseTake_WhenNegative_ShouldThrow()
    {
        var ex = Assert.Throws<ShorthandParseException>(() => ShorthandParamParser.ParseTake(Q(("take", "-1"))));
        Assert.Contains("take", ex.Message);
    }

    [Fact]
    public void ParseTake_WhenGibberish_ShouldThrow()
    {
        var ex = Assert.Throws<ShorthandParseException>(() => ShorthandParamParser.ParseTake(Q(("take", "banana"))));
        Assert.Contains("take", ex.Message);
    }

    [Fact]
    public void ParseWaitMs_WhenMissing_ShouldReturnZero()
    {
        var d = ShorthandParamParser.ParseWaitMs(Q());
        Assert.Equal(0, d.Milliseconds);
    }

    [Fact]
    public void ParseWaitMs_WhenValid_ShouldReturnSameValue()
    {
        Assert.Equal(5000, ShorthandParamParser.ParseWaitMs(Q(("wait_ms", "5000"))).Milliseconds);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("60001")]
    [InlineData("not-a-number")]
    public void ParseWaitMs_WhenOutOfRangeOrMalformed_ShouldThrow(string input)
    {
        Assert.Throws<ShorthandParseException>(() => ShorthandParamParser.ParseWaitMs(Q(("wait_ms", input))));
    }

    [Fact]
    public void TryGetAttributeKey_WhenAttrPrefix_ShouldReturnKey()
    {
        Assert.True(ShorthandParamParser.TryGetAttributeKey("attr.service.name", out var key));
        Assert.Equal("service.name", key);
    }

    [Fact]
    public void TryGetAttributeKey_WhenOnlyPrefix_ShouldReturnFalse()
    {
        Assert.False(ShorthandParamParser.TryGetAttributeKey("attr.", out _));
    }

    [Fact]
    public void TryGetAttributeKey_WhenNoPrefix_ShouldReturnFalse()
    {
        Assert.False(ShorthandParamParser.TryGetAttributeKey("name", out _));
    }

    [Fact]
    public void IsReservedParam_ShouldFlagTakeAndWaitMs()
    {
        Assert.True(ShorthandParamParser.IsReservedParam("take"));
        Assert.True(ShorthandParamParser.IsReservedParam("wait_ms"));
        Assert.False(ShorthandParamParser.IsReservedParam("name"));
    }
}
