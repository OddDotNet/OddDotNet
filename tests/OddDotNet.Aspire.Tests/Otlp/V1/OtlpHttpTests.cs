using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

using Google.Protobuf;

using OddDotNet.Proto.Common.V1;

using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Logs.V1;
using OpenTelemetry.Proto.Metrics.V1;
using OpenTelemetry.Proto.Resource.V1;
using OpenTelemetry.Proto.Trace.V1;

using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;

using TraceWhere = OddDotNet.Proto.Trace.V1.Where;
using TracePropertyFilter = OddDotNet.Proto.Trace.V1.PropertyFilter;
using TraceQueryRequest = OddDotNet.Proto.Trace.V1.SpanQueryRequest;

using MetricWhere = OddDotNet.Proto.Metrics.V1.Where;
using MetricPropertyFilter = OddDotNet.Proto.Metrics.V1.PropertyFilter;
using MetricQueryRequest = OddDotNet.Proto.Metrics.V1.MetricQueryRequest;

using LogWhere = OddDotNet.Proto.Logs.V1.Where;
using LogPropertyFilter = OddDotNet.Proto.Logs.V1.PropertyFilter;
using LogQueryRequest = OddDotNet.Proto.Logs.V1.LogQueryRequest;

namespace OddDotNet.Aspire.Tests.Otlp.V1;

public class OtlpHttpTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public OtlpHttpTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    private static ExportTraceServiceRequest BuildTraceRequest(string spanName)
    {
        return new ExportTraceServiceRequest
        {
            ResourceSpans =
            {
                new ResourceSpans
                {
                    Resource = new Resource
                    {
                        Attributes = { new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = "otlp-http-test" } } }
                    },
                    SchemaUrl = "https://schemas/resource",
                    ScopeSpans =
                    {
                        new ScopeSpans
                        {
                            Scope = new InstrumentationScope { Name = "scope-a", Version = "1.0" },
                            SchemaUrl = "https://schemas/scope",
                            Spans =
                            {
                                new OtelSpan
                                {
                                    Name = spanName,
                                    TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()),
                                    SpanId = ByteString.CopyFrom(new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 })
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static ExportMetricsServiceRequest BuildMetricRequest(string metricName)
    {
        return new ExportMetricsServiceRequest
        {
            ResourceMetrics =
            {
                new ResourceMetrics
                {
                    Resource = new Resource(),
                    ScopeMetrics =
                    {
                        new ScopeMetrics
                        {
                            Scope = new InstrumentationScope { Name = "scope-m" },
                            Metrics =
                            {
                                new Metric
                                {
                                    Name = metricName,
                                    Sum = new Sum
                                    {
                                        DataPoints =
                                        {
                                            new NumberDataPoint { AsDouble = 1.0 }
                                        },
                                        IsMonotonic = true,
                                        AggregationTemporality = AggregationTemporality.Cumulative
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static ExportLogsServiceRequest BuildLogRequest(string logBody)
    {
        return new ExportLogsServiceRequest
        {
            ResourceLogs =
            {
                new ResourceLogs
                {
                    Resource = new Resource(),
                    ScopeLogs =
                    {
                        new ScopeLogs
                        {
                            Scope = new InstrumentationScope { Name = "scope-l" },
                            LogRecords =
                            {
                                new LogRecord
                                {
                                    Body = new AnyValue { StringValue = logBody },
                                    SeverityNumber = SeverityNumber.Info
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private async Task AssertSpanQueryableByName(string spanName)
    {
        var filter = new TraceWhere
        {
            Property = new TracePropertyFilter
            {
                Name = new StringProperty { CompareAs = StringCompareAsType.Equals, Compare = spanName }
            }
        };
        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new TraceQueryRequest { Filters = { filter } });
        Assert.Contains(response.Spans, s => s.Span.Name == spanName);
    }

    private async Task AssertMetricQueryableByName(string metricName)
    {
        var filter = new MetricWhere
        {
            Property = new MetricPropertyFilter
            {
                Name = new StringProperty { CompareAs = StringCompareAsType.Equals, Compare = metricName }
            }
        };
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(new MetricQueryRequest { Filters = { filter } });
        Assert.Contains(response.Metrics, m => m.Metric.Name == metricName);
    }

    private async Task AssertLogQueryableByBody(string body)
    {
        var filter = new LogWhere
        {
            Property = new LogPropertyFilter
            {
                Body = new AnyValueProperty
                {
                    StringValue = new StringProperty { CompareAs = StringCompareAsType.Equals, Compare = body }
                }
            }
        };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(new LogQueryRequest { Filters = { filter } });
        Assert.Contains(response.Logs, l => l.Log.Body.StringValue == body);
    }

    private static ByteArrayContent ProtoContent(IMessage message)
    {
        var content = new ByteArrayContent(message.ToByteArray());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");
        return content;
    }

    private static StringContent JsonContent(IMessage message)
    {
        var json = JsonFormatter.Default.Format(message);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static byte[] Gzip(byte[] input)
    {
        using var ms = new MemoryStream();
        using (var gzip = new GZipStream(ms, CompressionMode.Compress, leaveOpen: true))
        {
            gzip.Write(input, 0, input.Length);
        }
        return ms.ToArray();
    }

    private static byte[] Deflate(byte[] input)
    {
        using var ms = new MemoryStream();
        using (var deflate = new DeflateStream(ms, CompressionMode.Compress, leaveOpen: true))
        {
            deflate.Write(input, 0, input.Length);
        }
        return ms.ToArray();
    }

    // ===== Traces =====

    [Fact]
    public async Task PostTracesProto_WhenWellFormed_ShouldBeQueryable()
    {
        var name = $"otlp-http-proto-trace-{Guid.NewGuid():N}";
        using var content = ProtoContent(BuildTraceRequest(name));

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/x-protobuf", response.Content.Headers.ContentType?.MediaType);
        await AssertSpanQueryableByName(name);
    }

    [Fact]
    public async Task PostTracesJson_WhenWellFormed_ShouldBeQueryable()
    {
        var name = $"otlp-http-json-trace-{Guid.NewGuid():N}";
        using var content = JsonContent(BuildTraceRequest(name));

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        await AssertSpanQueryableByName(name);
    }

    [Fact]
    public async Task PostTracesProtoGzip_WhenWellFormed_ShouldBeQueryable()
    {
        var name = $"otlp-http-gzip-trace-{Guid.NewGuid():N}";
        var compressed = Gzip(BuildTraceRequest(name).ToByteArray());
        using var content = new ByteArrayContent(compressed);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");
        content.Headers.ContentEncoding.Add("gzip");

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertSpanQueryableByName(name);
    }

    [Fact]
    public async Task PostTracesProtoDeflate_WhenWellFormed_ShouldBeQueryable()
    {
        var name = $"otlp-http-deflate-trace-{Guid.NewGuid():N}";
        var compressed = Deflate(BuildTraceRequest(name).ToByteArray());
        using var content = new ByteArrayContent(compressed);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");
        content.Headers.ContentEncoding.Add("deflate");

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertSpanQueryableByName(name);
    }

    [Fact]
    public async Task PostTracesProto_WhenMultipleResourceScopeSpans_ShouldProduceAllFlatSpans()
    {
        // 2 resources x 2 scopes x 3 spans = 12
        var prefix = $"otlp-http-multi-{Guid.NewGuid():N}";
        var request = new ExportTraceServiceRequest();
        for (int r = 0; r < 2; r++)
        {
            var rs = new ResourceSpans { Resource = new Resource() };
            for (int s = 0; s < 2; s++)
            {
                var ss = new ScopeSpans { Scope = new InstrumentationScope { Name = $"scope-{r}-{s}" } };
                for (int sp = 0; sp < 3; sp++)
                {
                    ss.Spans.Add(new OtelSpan
                    {
                        Name = $"{prefix}-{r}-{s}-{sp}",
                        TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()),
                        SpanId = ByteString.CopyFrom(new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 })
                    });
                }
                rs.ScopeSpans.Add(ss);
            }
            request.ResourceSpans.Add(rs);
        }

        using var content = ProtoContent(request);
        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var filter = new TraceWhere
        {
            Property = new TracePropertyFilter
            {
                Name = new StringProperty { CompareAs = StringCompareAsType.Contains, Compare = prefix }
            }
        };
        var queryResp = await _fixture.SpanQueryServiceClient.QueryAsync(new TraceQueryRequest
        {
            Filters = { filter },
            Take = new OddDotNet.Proto.Common.V1.Take { TakeAll = new OddDotNet.Proto.Common.V1.TakeAll() }
        });
        Assert.Equal(12, queryResp.Spans.Count);
    }

    // ===== Metrics =====

    [Fact]
    public async Task PostMetricsProto_WhenWellFormed_ShouldBeQueryable()
    {
        var name = $"otlp-http-proto-metric-{Guid.NewGuid():N}";
        using var content = ProtoContent(BuildMetricRequest(name));

        var response = await _fixture.HttpClient.PostAsync("/v1/metrics", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/x-protobuf", response.Content.Headers.ContentType?.MediaType);
        await AssertMetricQueryableByName(name);
    }

    [Fact]
    public async Task PostMetricsJson_WhenWellFormed_ShouldBeQueryable()
    {
        var name = $"otlp-http-json-metric-{Guid.NewGuid():N}";
        using var content = JsonContent(BuildMetricRequest(name));

        var response = await _fixture.HttpClient.PostAsync("/v1/metrics", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        await AssertMetricQueryableByName(name);
    }

    [Fact]
    public async Task PostMetricsJsonDeflate_WhenWellFormed_ShouldBeQueryable()
    {
        var name = $"otlp-http-deflate-metric-{Guid.NewGuid():N}";
        var json = JsonFormatter.Default.Format(BuildMetricRequest(name));
        var compressed = Deflate(Encoding.UTF8.GetBytes(json));
        using var content = new ByteArrayContent(compressed);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        content.Headers.ContentEncoding.Add("deflate");

        var response = await _fixture.HttpClient.PostAsync("/v1/metrics", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertMetricQueryableByName(name);
    }

    // ===== Logs =====

    [Fact]
    public async Task PostLogsProto_WhenWellFormed_ShouldBeQueryable()
    {
        var body = $"otlp-http-proto-log-{Guid.NewGuid():N}";
        using var content = ProtoContent(BuildLogRequest(body));

        var response = await _fixture.HttpClient.PostAsync("/v1/logs", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/x-protobuf", response.Content.Headers.ContentType?.MediaType);
        await AssertLogQueryableByBody(body);
    }

    [Fact]
    public async Task PostLogsJson_WhenWellFormed_ShouldBeQueryable()
    {
        var body = $"otlp-http-json-log-{Guid.NewGuid():N}";
        using var content = JsonContent(BuildLogRequest(body));

        var response = await _fixture.HttpClient.PostAsync("/v1/logs", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        await AssertLogQueryableByBody(body);
    }

    [Fact]
    public async Task PostLogsJsonGzip_WhenWellFormed_ShouldBeQueryable()
    {
        var body = $"otlp-http-gzip-log-{Guid.NewGuid():N}";
        var json = JsonFormatter.Default.Format(BuildLogRequest(body));
        var compressed = Gzip(Encoding.UTF8.GetBytes(json));
        using var content = new ByteArrayContent(compressed);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        content.Headers.ContentEncoding.Add("gzip");

        var response = await _fixture.HttpClient.PostAsync("/v1/logs", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertLogQueryableByBody(body);
    }

    // ===== Error cases =====

    [Theory]
    [InlineData("/v1/traces")]
    [InlineData("/v1/metrics")]
    [InlineData("/v1/logs")]
    public async Task Post_WhenTextPlainContentType_ShouldReturn415(string endpoint)
    {
        using var content = new StringContent("hi", Encoding.UTF8, "text/plain");

        var response = await _fixture.HttpClient.PostAsync(endpoint, content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Theory]
    [InlineData("/v1/traces")]
    [InlineData("/v1/metrics")]
    [InlineData("/v1/logs")]
    public async Task Post_WhenMalformedProtobuf_ShouldReturn400(string endpoint)
    {
        using var content = new ByteArrayContent(new byte[] { 0xFF, 0xFE, 0xFD, 0xFC, 0xFB });
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        var response = await _fixture.HttpClient.PostAsync(endpoint, content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/v1/traces")]
    [InlineData("/v1/metrics")]
    [InlineData("/v1/logs")]
    public async Task Post_WhenMalformedJson_ShouldReturn400(string endpoint)
    {
        using var content = new StringContent("{ invalid", Encoding.UTF8, "application/json");

        var response = await _fixture.HttpClient.PostAsync(endpoint, content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/v1/traces")]
    [InlineData("/v1/metrics")]
    [InlineData("/v1/logs")]
    public async Task Post_WhenEmptyBody_ShouldReturn400(string endpoint)
    {
        using var content = new ByteArrayContent(Array.Empty<byte>());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        var response = await _fixture.HttpClient.PostAsync(endpoint, content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ===== gRPC path unchanged =====

    [Fact]
    public async Task GrpcTraceExport_StillWorksAfterHttpEndpointsAdded()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var uniqueName = $"grpc-trace-unchanged-{Guid.NewGuid():N}";
        request.ResourceSpans[0].ScopeSpans[0].Spans[0].Name = uniqueName;

        await _fixture.TraceServiceClient.ExportAsync(request);

        await AssertSpanQueryableByName(uniqueName);
    }
}
