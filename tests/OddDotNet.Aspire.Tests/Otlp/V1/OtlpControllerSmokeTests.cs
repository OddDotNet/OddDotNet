using System.Net;
using System.Net.Http.Headers;

using Google.Protobuf;

using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Otlp.V1;

public class OtlpControllerSmokeTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public OtlpControllerSmokeTests(AspireFixture fixture)
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
                        Attributes = { new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = "smoke" } } }
                    },
                    ScopeSpans =
                    {
                        new ScopeSpans
                        {
                            Scope = new InstrumentationScope { Name = "smoke-scope" },
                            Spans =
                            {
                                new Span
                                {
                                    Name = spanName,
                                    TraceId = ByteString.CopyFrom(new byte[16]),
                                    SpanId = ByteString.CopyFrom(new byte[8])
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    [Fact]
    public async Task PostTraces_WhenProtobuf_ShouldReturnOkWithProtobufResponse()
    {
        var request = BuildTraceRequest($"smoke-proto-{Guid.NewGuid():N}");
        using var content = new ByteArrayContent(request.ToByteArray());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/x-protobuf", response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var parsed = ExportTraceServiceResponse.Parser.ParseFrom(bytes);
        Assert.NotNull(parsed);
    }

    [Fact]
    public async Task PostTraces_WhenJson_ShouldReturnOkWithJsonResponse()
    {
        var request = BuildTraceRequest($"smoke-json-{Guid.NewGuid():N}");
        var json = JsonFormatter.Default.Format(request);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task PostTraces_WhenUnsupportedContentType_ShouldReturn415()
    {
        using var content = new StringContent("hi", System.Text.Encoding.UTF8, "text/plain");

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task PostTraces_WhenMalformedProtobuf_ShouldReturn400()
    {
        using var content = new ByteArrayContent(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTraces_WhenEmptyBody_ShouldReturn400()
    {
        using var content = new ByteArrayContent(Array.Empty<byte>());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        var response = await _fixture.HttpClient.PostAsync("/v1/traces", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
