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

public class QueryControllerTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public QueryControllerTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PostSpans_WhenFilterMatches_ShouldReturnSpan()
    {
        var uniqueName = $"query-ctrl-{Guid.NewGuid():N}";
        var exportReq = new ExportTraceServiceRequest
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
                            Scope = new InstrumentationScope { Name = "s" },
                            Spans =
                            {
                                new OtelSpan
                                {
                                    Name = uniqueName,
                                    TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()),
                                    SpanId = ByteString.CopyFrom(new byte[8] {1,2,3,4,5,6,7,8})
                                }
                            }
                        }
                    }
                }
            }
        };
        await _fixture.TraceServiceClient.ExportAsync(exportReq);

        var queryReq = new OddDotNet.Proto.Trace.V1.SpanQueryRequest
        {
            Take = new OddDotNet.Proto.Common.V1.Take { TakeAll = new OddDotNet.Proto.Common.V1.TakeAll() },
            Duration = new OddDotNet.Proto.Common.V1.Duration { Milliseconds = 500 },
            Filters =
            {
                new OddDotNet.Proto.Trace.V1.Where
                {
                    Property = new OddDotNet.Proto.Trace.V1.PropertyFilter
                    {
                        Name = new OddDotNet.Proto.Common.V1.StringProperty
                        {
                            CompareAs = OddDotNet.Proto.Common.V1.StringCompareAsType.Equals,
                            Compare = uniqueName
                        }
                    }
                }
            }
        };
        using var content = new StringContent(JsonFormatter.Default.Format(queryReq), Encoding.UTF8, "application/json");

        var response = await _fixture.HttpClient.PostAsync("/query/v1/spans", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.Equal(uniqueName, root.GetProperty("items")[0].GetProperty("span").GetProperty("name").GetString());
    }

    [Fact]
    public async Task PostAppInsightsRequests_WhenFilterMatches_ShouldReturnRequest()
    {
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        var uniqueId = $"query-req-{Guid.NewGuid():N}";
        envelope.Data!.BaseData!.Id = uniqueId;
        using (var ingestContent = new StringContent(JsonSerializer.Serialize(envelope), Encoding.UTF8, "application/json"))
        {
            await _fixture.HttpClient.PostAsync("/v2/track", ingestContent);
        }

        var queryReq = new OddDotNet.Proto.AppInsights.V1.Request.RequestQueryRequest
        {
            Take = new OddDotNet.Proto.Common.V1.Take { TakeAll = new OddDotNet.Proto.Common.V1.TakeAll() },
            Duration = new OddDotNet.Proto.Common.V1.Duration { Milliseconds = 500 },
            Filters =
            {
                new OddDotNet.Proto.AppInsights.V1.Request.Where
                {
                    Property = new OddDotNet.Proto.AppInsights.V1.Request.PropertyFilter
                    {
                        Id = new OddDotNet.Proto.Common.V1.StringProperty
                        {
                            CompareAs = OddDotNet.Proto.Common.V1.StringCompareAsType.Equals,
                            Compare = uniqueId
                        }
                    }
                }
            }
        };
        using var content = new StringContent(JsonFormatter.Default.Format(queryReq), Encoding.UTF8, "application/json");

        var response = await _fixture.HttpClient.PostAsync("/query/v1/appinsights/requests", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task DeleteSpans_ShouldClearOnlySpans()
    {
        // Ingest a span and a log
        var span = new OtelSpan
        {
            Name = $"delete-span-{Guid.NewGuid():N}",
            TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()),
            SpanId = ByteString.CopyFrom(new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 })
        };
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

        var logReq = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(logReq);

        var deleteResponse = await _fixture.HttpClient.DeleteAsync("/query/v1/spans");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Spans should be empty but logs should still have data. Verify via /query/v1/*.
        var spanQueryBody = "{\"take\":{\"takeAll\":{}},\"duration\":{\"milliseconds\":100}}";
        using var spanContent = new StringContent(spanQueryBody, Encoding.UTF8, "application/json");
        var spanResp = await _fixture.HttpClient.PostAsync("/query/v1/spans", spanContent);
        var spanRoot = JsonDocument.Parse(await spanResp.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(0, spanRoot.GetProperty("count").GetInt32());

        using var logContent = new StringContent(spanQueryBody, Encoding.UTF8, "application/json");
        var logResp = await _fixture.HttpClient.PostAsync("/query/v1/logs", logContent);
        var logRoot = JsonDocument.Parse(await logResp.Content.ReadAsStringAsync()).RootElement;
        Assert.True(logRoot.GetProperty("count").GetInt32() >= 1);
    }

    [Fact]
    public async Task Post_WhenUnknownSignal_ShouldReturn404()
    {
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/query/v1/widgets", content);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenTextPlainContentType_ShouldReturn415()
    {
        using var content = new StringContent("{}", Encoding.UTF8, "text/plain");
        var response = await _fixture.HttpClient.PostAsync("/query/v1/spans", content);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenMalformedJson_ShouldReturn400()
    {
        using var content = new StringContent("{ invalid", Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/query/v1/spans", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
