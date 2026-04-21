using System.Net;
using System.Text;
using System.Text.Json;

using Google.Protobuf;

using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;

using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OtelResourceSpans = OpenTelemetry.Proto.Trace.V1.ResourceSpans;
using OtelScopeSpans = OpenTelemetry.Proto.Trace.V1.ScopeSpans;

using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryGetShorthandTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;
    public QueryGetShorthandTests(AspireFixture fixture) { _fixture = fixture; }

    private async Task IngestSpan(string name, KeyValue[]? attrs = null)
    {
        var span = new OtelSpan
        {
            Name = name,
            TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()),
            SpanId = ByteString.CopyFrom(new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 })
        };
        if (attrs != null) span.Attributes.AddRange(attrs);

        await _fixture.TraceServiceClient.ExportAsync(new ExportTraceServiceRequest
        {
            ResourceSpans =
            {
                new OtelResourceSpans
                {
                    Resource = new Resource(),
                    ScopeSpans = { new OtelScopeSpans { Scope = new InstrumentationScope(), Spans = { span } } }
                }
            }
        });
    }

    private async Task IngestAi(AppInsightsTelemetryEnvelope env)
    {
        using var content = new StringContent(JsonSerializer.Serialize(env), Encoding.UTF8, "application/json");
        await _fixture.HttpClient.PostAsync("/v2/track", content);
    }

    private async Task<JsonElement> GetJson(string url)
    {
        var resp = await _fixture.HttpClient.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
    }

    [Fact]
    public async Task GetSpans_ByName_ReturnsMatch()
    {
        var name = $"get-name-{Guid.NewGuid():N}";
        await IngestSpan(name);

        var root = await GetJson($"/query/v1/spans?name={name}&take=all&wait_ms=200");
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task GetSpans_ByAttrDottedKey_ReturnsMatch()
    {
        var unique = $"svc-{Guid.NewGuid():N}";
        await IngestSpan($"attr-test-{unique}", new[]
        {
            new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = unique } }
        });

        var root = await GetJson($"/query/v1/spans?attr.service.name={unique}&take=all&wait_ms=500");
        Assert.True(root.GetProperty("count").GetInt32() >= 1);
    }

    [Fact]
    public async Task GetSpans_NameAndAttr_ShouldAND()
    {
        var prefix = Guid.NewGuid().ToString("N");
        var nameA = $"and-{prefix}-a";
        var nameB = $"and-{prefix}-b";
        await IngestSpan(nameA, new[] { new KeyValue { Key = "env", Value = new AnyValue { StringValue = "prod" } } });
        await IngestSpan(nameB, new[] { new KeyValue { Key = "env", Value = new AnyValue { StringValue = "dev" } } });

        var root = await GetJson($"/query/v1/spans?name={nameA}&attr.env=prod&take=all&wait_ms=500");
        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.Equal(nameA, root.GetProperty("items")[0].GetProperty("span").GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetAppInsightsRequests_ById_ReturnsMatch()
    {
        var env = AppInsightsHelpers.CreateRequestEnvelope();
        var uniqueId = $"get-req-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Id = uniqueId;
        await IngestAi(env);

        var root = await GetJson($"/query/v1/appinsights/requests?id={uniqueId}&take=all&wait_ms=500");
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task GetAppInsightsEvents_ByName_ReturnsMatch()
    {
        var env = AppInsightsHelpers.CreateEventEnvelope();
        var uniqueName = $"get-evt-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Name = uniqueName;
        await IngestAi(env);

        var root = await GetJson($"/query/v1/appinsights/events?name={uniqueName}&take=all&wait_ms=500");
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }
}
