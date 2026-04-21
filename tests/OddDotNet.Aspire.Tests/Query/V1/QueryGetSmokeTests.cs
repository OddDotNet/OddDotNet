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

public class QueryGetSmokeTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public QueryGetSmokeTests(AspireFixture fixture) { _fixture = fixture; }

    [Fact]
    public async Task GetSpans_WithNameParam_ReturnsMatchingSpan()
    {
        var name = $"get-smoke-{Guid.NewGuid():N}";
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
                            Spans =
                            {
                                new OtelSpan { Name = name, TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()), SpanId = ByteString.CopyFrom(new byte[8]{1,2,3,4,5,6,7,8}) }
                            }
                        }
                    }
                }
            }
        });

        var response = await _fixture.HttpClient.GetAsync($"/query/v1/spans?name={name}&take=all&wait_ms=200");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.Equal(name, root.GetProperty("items")[0].GetProperty("span").GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetUnknownSignal_Returns404()
    {
        var response = await _fixture.HttpClient.GetAsync("/query/v1/widgets?name=x");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSpans_WithUnknownField_Returns400()
    {
        var response = await _fixture.HttpClient.GetAsync("/query/v1/spans?favorite_color=blue");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSpans_WithMalformedTake_Returns400()
    {
        var response = await _fixture.HttpClient.GetAsync("/query/v1/spans?name=x&take=banana");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
