using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.Common.V1;

namespace OddDotNet.Services.Query.Shorthand;

public static class ShorthandParamParser
{
    public const string TakeParam = "take";
    public const string WaitMsParam = "wait_ms";
    public const string AttrPrefix = "attr.";

    public const int MaxWaitMs = 60000;

    public static Take ParseTake(IQueryCollection query)
    {
        if (!query.TryGetValue(TakeParam, out var raw) || raw.Count == 0)
        {
            return new Take { TakeFirst = new TakeFirst() };
        }

        var value = raw.ToString();

        if (string.Equals(value, "all", StringComparison.OrdinalIgnoreCase))
            return new Take { TakeAll = new TakeAll() };

        if (string.Equals(value, "first", StringComparison.OrdinalIgnoreCase))
            return new Take { TakeFirst = new TakeFirst() };

        if (int.TryParse(value, out int n))
        {
            if (n < 0)
                throw new ShorthandParseException($"'take' must be non-negative, got {n}");
            return new Take { TakeExact = new TakeExact { Count = n } };
        }

        throw new ShorthandParseException($"'take' must be an integer, 'all', or 'first'; got '{value}'");
    }

    public static Duration ParseWaitMs(IQueryCollection query)
    {
        if (!query.TryGetValue(WaitMsParam, out var raw) || raw.Count == 0)
        {
            return new Duration { Milliseconds = 0 };
        }

        var value = raw.ToString();
        if (!int.TryParse(value, out int ms))
            throw new ShorthandParseException($"'wait_ms' must be an integer; got '{value}'");

        if (ms < 0 || ms > MaxWaitMs)
            throw new ShorthandParseException($"'wait_ms' must be in [0, {MaxWaitMs}]; got {ms}");

        return new Duration { Milliseconds = ms };
    }

    public static bool TryGetAttributeKey(string paramName, out string attributeKey)
    {
        if (paramName.StartsWith(AttrPrefix, StringComparison.Ordinal) && paramName.Length > AttrPrefix.Length)
        {
            attributeKey = paramName.Substring(AttrPrefix.Length);
            return true;
        }
        attributeKey = string.Empty;
        return false;
    }

    public static bool IsReservedParam(string paramName) =>
        paramName.Equals(TakeParam, StringComparison.OrdinalIgnoreCase) ||
        paramName.Equals(WaitMsParam, StringComparison.OrdinalIgnoreCase);
}
