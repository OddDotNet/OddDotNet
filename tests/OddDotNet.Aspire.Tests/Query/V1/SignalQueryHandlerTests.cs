using System.Text.Json;

using Google.Protobuf;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;
using OddDotNet.Services.Query;

using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class SignalQueryHandlerTests
{
    private static SignalList<FlatSpan> NewSpanList()
    {
        var signals = new SignalList<FlatSpan>(
            new ChannelManager<FlatSpan>(),
            TimeProvider.System,
            NullLogger<SignalList<FlatSpan>>.Instance,
            Options.Create(new OddSettings()));
        signals.Reset();
        return signals;
    }

    private static SignalQueryHandler<SpanQueryRequest, FlatSpan> NewSpanHandler(SignalList<FlatSpan> signals) =>
        new(
            "spans",
            signals,
            req => req.Take,
            req => req.Duration,
            req => req.Filters);

    [Fact]
    public async Task QueryAsJsonAsync_WhenRequestHasShortDurationAndNoMatches_ShouldReturnEmptyArray()
    {
        var signals = NewSpanList();
        var handler = NewSpanHandler(signals);
        var request = new SpanQueryRequest
        {
            Take = new Take { TakeFirst = new TakeFirst() },
            Duration = new Duration { Milliseconds = 50 }
        };

        string json = await handler.QueryAsJsonAsync(JsonFormatter.Default.Format(request), CancellationToken.None);

        var root = JsonDocument.Parse(json).RootElement;
        Assert.Equal(0, root.GetProperty("items").GetArrayLength());
        Assert.Equal(0, root.GetProperty("count").GetInt32());
        Assert.False(root.GetProperty("truncated").GetBoolean());
    }

    [Fact]
    public async Task QueryAsJsonAsync_WhenFilterMatches_ShouldReturnMatchingItem()
    {
        var signals = NewSpanList();
        var handler = NewSpanHandler(signals);
        var uniqueName = $"unit-{Guid.NewGuid():N}";
        signals.Add(new FlatSpan { Span = new Span { Name = uniqueName, TraceId = ByteString.CopyFrom(new byte[16]), SpanId = ByteString.CopyFrom(new byte[8]) } });
        signals.Add(new FlatSpan { Span = new Span { Name = "other", TraceId = ByteString.CopyFrom(new byte[16]), SpanId = ByteString.CopyFrom(new byte[8]) } });

        var request = new SpanQueryRequest
        {
            Take = new Take { TakeAll = new TakeAll() },
            Duration = new Duration { Milliseconds = 50 },
            Filters =
            {
                new Where
                {
                    Property = new PropertyFilter
                    {
                        Name = new StringProperty { CompareAs = StringCompareAsType.Equals, Compare = uniqueName }
                    }
                }
            }
        };
        string body = JsonFormatter.Default.Format(request);

        string json = await handler.QueryAsJsonAsync(body, CancellationToken.None);

        var root = JsonDocument.Parse(json).RootElement;
        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.False(root.GetProperty("truncated").GetBoolean());
        Assert.Equal(uniqueName, root.GetProperty("items")[0].GetProperty("span").GetProperty("name").GetString());
    }

    [Fact]
    public async Task QueryAsJsonAsync_WhenTakeExactAndMoreAvailable_ShouldTruncate()
    {
        var signals = NewSpanList();
        var handler = NewSpanHandler(signals);
        var unique = $"truncate-{Guid.NewGuid():N}";
        for (int i = 0; i < 5; i++)
        {
            signals.Add(new FlatSpan { Span = new Span { Name = $"{unique}-{i}", TraceId = ByteString.CopyFrom(new byte[16]), SpanId = ByteString.CopyFrom(new byte[8]) } });
        }

        var request = new SpanQueryRequest
        {
            Take = new Take { TakeExact = new TakeExact { Count = 2 } },
            Filters =
            {
                new Where
                {
                    Property = new PropertyFilter
                    {
                        Name = new StringProperty { CompareAs = StringCompareAsType.Contains, Compare = unique }
                    }
                }
            }
        };

        string json = await handler.QueryAsJsonAsync(JsonFormatter.Default.Format(request), CancellationToken.None);

        var root = JsonDocument.Parse(json).RootElement;
        Assert.Equal(2, root.GetProperty("count").GetInt32());
        Assert.True(root.GetProperty("truncated").GetBoolean());
    }

    [Fact]
    public async Task QueryAsJsonAsync_WhenMalformedJson_ShouldThrow()
    {
        var handler = NewSpanHandler(NewSpanList());

        await Assert.ThrowsAsync<InvalidJsonException>(() =>
            handler.QueryAsJsonAsync("{ not valid", CancellationToken.None));
    }

    [Fact]
    public void Reset_ShouldClearUnderlyingSignalList()
    {
        var signals = NewSpanList();
        var handler = NewSpanHandler(signals);
        signals.Add(new FlatSpan { Span = new Span { Name = "x", TraceId = ByteString.CopyFrom(new byte[16]), SpanId = ByteString.CopyFrom(new byte[8]) } });
        Assert.Equal(1, signals.Count);

        handler.Reset();

        Assert.Equal(0, signals.Count);
    }

    [Fact]
    public void SignalPath_ShouldExposeConstructorArgument()
    {
        var handler = NewSpanHandler(NewSpanList());
        Assert.Equal("spans", handler.SignalPath);
    }
}
