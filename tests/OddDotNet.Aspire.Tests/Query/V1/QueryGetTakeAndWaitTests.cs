using System.Diagnostics;
using System.Net;
using System.Text.Json;

using Google.Protobuf;

using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;

using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OtelResourceSpans = OpenTelemetry.Proto.Trace.V1.ResourceSpans;
using OtelScopeSpans = OpenTelemetry.Proto.Trace.V1.ScopeSpans;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryGetTakeAndWaitTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;
    public QueryGetTakeAndWaitTests(AspireFixture fixture) { _fixture = fixture; }

    private async Task IngestN(string baseName, int n)
    {
        for (int i = 0; i < n; i++)
        {
            await _fixture.TraceServiceClient.ExportAsync(new ExportTraceServiceRequest
            {
                ResourceSpans =
                {
                    new OtelResourceSpans
                    {
                        Resource = new Resource(),
                        ScopeSpans =
                        {
                            new OtelScopeSpans
                            {
                                Scope = new InstrumentationScope(),
                                Spans = { new OtelSpan { Name = baseName, TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()), SpanId = ByteString.CopyFrom(new byte[8]{1,2,3,4,5,6,7,8}) } }
                            }
                        }
                    }
                }
            });
        }
    }

    private async Task<JsonElement> Get(string url)
    {
        var resp = await _fixture.HttpClient.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
    }

    [Fact]
    public async Task TakeN_CapsAndMarksTruncated()
    {
        var name = $"take-n-{Guid.NewGuid():N}";
        await IngestN(name, 5);

        var root = await Get($"/query/v1/spans?name={name}&take=3&wait_ms=500");
        Assert.Equal(3, root.GetProperty("count").GetInt32());
        Assert.True(root.GetProperty("truncated").GetBoolean());
    }

    [Fact]
    public async Task TakeAll_ReturnsEverything()
    {
        var name = $"take-all-{Guid.NewGuid():N}";
        await IngestN(name, 4);

        var root = await Get($"/query/v1/spans?name={name}&take=all&wait_ms=500");
        Assert.Equal(4, root.GetProperty("count").GetInt32());
        Assert.False(root.GetProperty("truncated").GetBoolean());
    }

    [Fact]
    public async Task TakeFirst_ReturnsOne()
    {
        var name = $"take-first-{Guid.NewGuid():N}";
        await IngestN(name, 3);

        var root = await Get($"/query/v1/spans?name={name}&take=first&wait_ms=500");
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task TakeZero_ReturnsZero()
    {
        var name = $"take-zero-{Guid.NewGuid():N}";
        await IngestN(name, 3);

        var root = await Get($"/query/v1/spans?name={name}&take=0&wait_ms=500");
        Assert.Equal(0, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task DefaultTake_ReturnsOne()
    {
        var name = $"take-default-{Guid.NewGuid():N}";
        await IngestN(name, 3);

        var root = await Get($"/query/v1/spans?name={name}&wait_ms=500");
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task WaitMs_ReturnsEarlyOnMatch()
    {
        var name = $"wait-hit-{Guid.NewGuid():N}";

        var sw = Stopwatch.StartNew();
        var queryTask = _fixture.HttpClient.GetAsync($"/query/v1/spans?name={name}&take=first&wait_ms=5000");

        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            await IngestN(name, 1);
        });

        var resp = await queryTask;
        sw.Stop();
        resp.EnsureSuccessStatusCode();
        var root = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.True(sw.ElapsedMilliseconds < 4000, $"returned in {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task WaitMsZero_ReturnsQuickly()
    {
        var sw = Stopwatch.StartNew();
        var root = await Get($"/query/v1/spans?name=never-{Guid.NewGuid():N}&wait_ms=0");
        sw.Stop();
        Assert.Equal(0, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task WaitMsOutOfRange_Returns400()
    {
        var resp = await _fixture.HttpClient.GetAsync("/query/v1/spans?name=x&wait_ms=60001");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task MalformedTake_Returns400()
    {
        var resp = await _fixture.HttpClient.GetAsync("/query/v1/spans?name=x&take=banana");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }
}
