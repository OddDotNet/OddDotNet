using System.Text;
using System.Text.Json;

using Google.Protobuf;

using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;

using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OtelResourceSpans = OpenTelemetry.Proto.Trace.V1.ResourceSpans;
using OtelScopeSpans = OpenTelemetry.Proto.Trace.V1.ScopeSpans;

using Common = OddDotNet.Proto.Common.V1;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryGetParityTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;
    public QueryGetParityTests(AspireFixture fixture) { _fixture = fixture; }

    [Fact]
    public async Task SpansByName_GetAndPost_ReturnSameResults()
    {
        var name = $"parity-{Guid.NewGuid():N}";
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
                            Spans = { new OtelSpan { Name = name, TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()), SpanId = ByteString.CopyFrom(new byte[8]{1,2,3,4,5,6,7,8}) } }
                        }
                    }
                }
            }
        });

        // GET
        var getResp = await _fixture.HttpClient.GetAsync($"/query/v1/spans?name={name}&take=all&wait_ms=500");
        getResp.EnsureSuccessStatusCode();
        var getRoot = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync()).RootElement;

        // POST with equivalent filter
        var postReq = new OddDotNet.Proto.Trace.V1.SpanQueryRequest
        {
            Take = new Common.Take { TakeAll = new Common.TakeAll() },
            Duration = new Common.Duration { Milliseconds = 500 },
            Filters =
            {
                new OddDotNet.Proto.Trace.V1.Where
                {
                    Property = new OddDotNet.Proto.Trace.V1.PropertyFilter
                    {
                        Name = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = name }
                    }
                }
            }
        };
        using var postContent = new StringContent(JsonFormatter.Default.Format(postReq), Encoding.UTF8, "application/json");
        var postResp = await _fixture.HttpClient.PostAsync("/query/v1/spans", postContent);
        postResp.EnsureSuccessStatusCode();
        var postRoot = JsonDocument.Parse(await postResp.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(getRoot.GetProperty("count").GetInt32(), postRoot.GetProperty("count").GetInt32());
        Assert.Equal(getRoot.GetProperty("truncated").GetBoolean(), postRoot.GetProperty("truncated").GetBoolean());
    }

    [Fact]
    public async Task AppInsightsRequestsById_GetAndPost_ReturnSameResults()
    {
        var env = AppInsightsHelpers.CreateRequestEnvelope();
        var id = $"parity-req-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Id = id;
        using (var content = new StringContent(JsonSerializer.Serialize(env), Encoding.UTF8, "application/json"))
        {
            await _fixture.HttpClient.PostAsync("/v2/track", content);
        }

        var getResp = await _fixture.HttpClient.GetAsync($"/query/v1/appinsights/requests?id={id}&take=all&wait_ms=500");
        getResp.EnsureSuccessStatusCode();
        var getRoot = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync()).RootElement;

        var postReq = new OddDotNet.Proto.AppInsights.V1.Request.RequestQueryRequest
        {
            Take = new Common.Take { TakeAll = new Common.TakeAll() },
            Duration = new Common.Duration { Milliseconds = 500 },
            Filters =
            {
                new OddDotNet.Proto.AppInsights.V1.Request.Where
                {
                    Property = new OddDotNet.Proto.AppInsights.V1.Request.PropertyFilter
                    {
                        Id = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = id }
                    }
                }
            }
        };
        using var postContent = new StringContent(JsonFormatter.Default.Format(postReq), Encoding.UTF8, "application/json");
        var postResp = await _fixture.HttpClient.PostAsync("/query/v1/appinsights/requests", postContent);
        postResp.EnsureSuccessStatusCode();
        var postRoot = JsonDocument.Parse(await postResp.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(getRoot.GetProperty("count").GetInt32(), postRoot.GetProperty("count").GetInt32());
    }
}
