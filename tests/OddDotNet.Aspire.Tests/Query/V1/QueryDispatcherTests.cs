using OddDotNet.Services.Query;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryDispatcherTests
{
    private sealed class StubHandler : ISignalQueryHandler
    {
        public StubHandler(string path) { SignalPath = path; }
        public string SignalPath { get; }
        public bool SupportsGetShorthand => false;
        public int ResetCalls { get; private set; }
        public Task<string> QueryAsJsonAsync(string jsonBody, CancellationToken ct) => Task.FromResult("{}");
        public Task<string> QueryAsJsonFromQueryStringAsync(Microsoft.AspNetCore.Http.IQueryCollection query, CancellationToken ct) => Task.FromResult("{}");
        public void Reset() => ResetCalls++;
    }

    [Fact]
    public void TryGet_WhenKnownPath_ShouldReturnHandler()
    {
        var dispatcher = new QueryDispatcher(new[] { new StubHandler("spans"), new StubHandler("logs") });

        var found = dispatcher.TryGet("spans", out var handler);

        Assert.True(found);
        Assert.Equal("spans", handler!.SignalPath);
    }

    [Fact]
    public void TryGet_WhenUnknownPath_ShouldReturnFalse()
    {
        var dispatcher = new QueryDispatcher(new[] { new StubHandler("spans") });

        var found = dispatcher.TryGet("widgets", out var handler);

        Assert.False(found);
        Assert.Null(handler);
    }

    [Fact]
    public void TryGet_ShouldBeCaseInsensitive()
    {
        var dispatcher = new QueryDispatcher(new[] { new StubHandler("appinsights/requests") });

        Assert.True(dispatcher.TryGet("AppInsights/Requests", out _));
    }

    [Fact]
    public void ResetAll_ShouldCallResetOnEveryHandler()
    {
        var a = new StubHandler("a");
        var b = new StubHandler("b");
        var c = new StubHandler("c");
        var dispatcher = new QueryDispatcher(new[] { a, b, c });

        dispatcher.ResetAll();

        Assert.Equal(1, a.ResetCalls);
        Assert.Equal(1, b.ResetCalls);
        Assert.Equal(1, c.ResetCalls);
    }

    [Fact]
    public void KnownPaths_ShouldExposeAllRegisteredSignalPaths()
    {
        var dispatcher = new QueryDispatcher(new[] { new StubHandler("spans"), new StubHandler("logs") });

        Assert.Contains("spans", dispatcher.KnownPaths);
        Assert.Contains("logs", dispatcher.KnownPaths);
        Assert.Equal(2, dispatcher.KnownPaths.Count);
    }
}
