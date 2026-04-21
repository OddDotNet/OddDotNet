using System.Net;
using System.Text;
using System.Text.Json;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryResetTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public QueryResetTests(AspireFixture fixture) { _fixture = fixture; }

    [Theory]
    [InlineData("spans")]
    [InlineData("metrics")]
    [InlineData("logs")]
    [InlineData("appinsights/requests")]
    [InlineData("appinsights/dependencies")]
    [InlineData("appinsights/exceptions")]
    [InlineData("appinsights/traces")]
    [InlineData("appinsights/events")]
    [InlineData("appinsights/metrics")]
    [InlineData("appinsights/pageviews")]
    [InlineData("appinsights/availability")]
    public async Task Delete_EachSignal_Returns204(string signalPath)
    {
        var response = await _fixture.HttpClient.DeleteAsync($"/query/v1/{signalPath}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the signal is empty after reset
        var queryBody = "{\"take\":{\"takeAll\":{}},\"duration\":{\"milliseconds\":50}}";
        using var content = new StringContent(queryBody, Encoding.UTF8, "application/json");
        var queryResp = await _fixture.HttpClient.PostAsync($"/query/v1/{signalPath}", content);
        queryResp.EnsureSuccessStatusCode();
        var root = JsonDocument.Parse(await queryResp.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(0, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task DeleteAll_Returns204()
    {
        // Seed at least one item of two different types
        var req = new OpenTelemetry.Proto.Collector.Trace.V1.ExportTraceServiceRequest
        {
            ResourceSpans =
            {
                new OpenTelemetry.Proto.Trace.V1.ResourceSpans
                {
                    Resource = new OpenTelemetry.Proto.Resource.V1.Resource(),
                    ScopeSpans =
                    {
                        new OpenTelemetry.Proto.Trace.V1.ScopeSpans
                        {
                            Scope = new OpenTelemetry.Proto.Common.V1.InstrumentationScope(),
                            Spans =
                            {
                                new OpenTelemetry.Proto.Trace.V1.Span
                                {
                                    Name = $"reset-all-{Guid.NewGuid():N}",
                                    TraceId = Google.Protobuf.ByteString.CopyFrom(Guid.NewGuid().ToByteArray()),
                                    SpanId = Google.Protobuf.ByteString.CopyFrom(new byte[8]{1,2,3,4,5,6,7,8})
                                }
                            }
                        }
                    }
                }
            }
        };
        await _fixture.TraceServiceClient.ExportAsync(req);

        var response = await _fixture.HttpClient.DeleteAsync("/query/v1/all");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var queryBody = "{\"take\":{\"takeAll\":{}},\"duration\":{\"milliseconds\":50}}";
        using var content = new StringContent(queryBody, Encoding.UTF8, "application/json");
        var queryResp = await _fixture.HttpClient.PostAsync("/query/v1/spans", content);
        var root = JsonDocument.Parse(await queryResp.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(0, root.GetProperty("count").GetInt32());
    }
}
