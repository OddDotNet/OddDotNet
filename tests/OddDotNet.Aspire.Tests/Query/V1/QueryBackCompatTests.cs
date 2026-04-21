using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Google.Protobuf;

using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;

using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OtelResourceSpans = OpenTelemetry.Proto.Trace.V1.ResourceSpans;
using OtelScopeSpans = OpenTelemetry.Proto.Trace.V1.ScopeSpans;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryBackCompatTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public QueryBackCompatTests(AspireFixture fixture) { _fixture = fixture; }

    [Fact]
    public async Task ExistingGrpcSpanQueryService_StillWorks()
    {
        var name = $"backcompat-grpc-{Guid.NewGuid():N}";
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

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new OddDotNet.Proto.Trace.V1.SpanQueryRequest
        {
            Filters = { new OddDotNet.Proto.Trace.V1.Where { Property = new OddDotNet.Proto.Trace.V1.PropertyFilter { Name = new OddDotNet.Proto.Common.V1.StringProperty { CompareAs = OddDotNet.Proto.Common.V1.StringCompareAsType.Equals, Compare = name } } } }
        });

        Assert.Single(response.Spans);
    }

    [Fact]
    public async Task ExistingAppInsightsSummaryEndpoint_StillWorks()
    {
        var env = AppInsightsHelpers.CreateRequestEnvelope();
        env.Data!.BaseData!.Id = $"bc-{Guid.NewGuid():N}";
        using (var content = new StringContent(JsonSerializer.Serialize(env), Encoding.UTF8, "application/json"))
        {
            await _fixture.HttpClient.PostAsync("/v2/track", content);
        }

        var response = await _fixture.HttpClient.GetAsync("/appinsights");
        response.EnsureSuccessStatusCode();
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(root.GetProperty("requests").GetInt32() >= 1);
    }

    [Fact]
    public async Task ExistingAppInsightsResetEndpoint_StillWorks()
    {
        var response = await _fixture.HttpClient.DeleteAsync("/appinsights/reset");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExistingOtlpHttpTracesEndpoint_StillWorks()
    {
        var request = new ExportTraceServiceRequest
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
                            Spans = { new OtelSpan { Name = $"bc-otlp-{Guid.NewGuid():N}", TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()), SpanId = ByteString.CopyFrom(new byte[8]{1,2,3,4,5,6,7,8}) } }
                        }
                    }
                }
            }
        };
        using var content = new ByteArrayContent(request.ToByteArray());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
