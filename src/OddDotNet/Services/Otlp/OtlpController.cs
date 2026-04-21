using System.IO.Compression;

using Google.Protobuf;

using Microsoft.AspNetCore.Mvc;

using OddDotNet.Proto.Logs.V1;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Trace.V1;

using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace OddDotNet.Services.Otlp;

[ApiController]
public class OtlpController : ControllerBase
{
    private const string ProtobufContentType = "application/x-protobuf";
    private const string JsonContentType = "application/json";

    private readonly SignalList<FlatSpan> _spans;
    private readonly SignalList<FlatMetric> _metrics;
    private readonly SignalList<FlatLog> _logs;
    private readonly ILogger<OtlpController> _logger;

    public OtlpController(
        SignalList<FlatSpan> spans,
        SignalList<FlatMetric> metrics,
        SignalList<FlatLog> logs,
        ILogger<OtlpController> logger)
    {
        _spans = spans;
        _metrics = metrics;
        _logs = logs;
        _logger = logger;
    }

    [HttpPost("/v1/traces")]
    public Task<IActionResult> Traces() =>
        HandleAsync(
            ExportTraceServiceRequest.Parser,
            req => OtlpFlattener.Flatten(req, _spans),
            () => new ExportTraceServiceResponse());

    [HttpPost("/v1/metrics")]
    public Task<IActionResult> Metrics() =>
        HandleAsync(
            ExportMetricsServiceRequest.Parser,
            req => OtlpFlattener.Flatten(req, _metrics),
            () => new ExportMetricsServiceResponse());

    [HttpPost("/v1/logs")]
    public Task<IActionResult> Logs() =>
        HandleAsync(
            ExportLogsServiceRequest.Parser,
            req => OtlpFlattener.Flatten(req, _logs),
            () => new ExportLogsServiceResponse());

    private async Task<IActionResult> HandleAsync<TRequest, TResponse>(
        MessageParser<TRequest> parser,
        Action<TRequest> ingest,
        Func<TResponse> responseFactory)
        where TRequest : IMessage<TRequest>, new()
        where TResponse : IMessage<TResponse>
    {
        var contentType = Request.ContentType ?? string.Empty;
        var format = DetectFormat(contentType);
        if (format is null)
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        byte[] body;
        try
        {
            body = await ReadBodyAsync();
        }
        catch (InvalidDataException ex)
        {
            _logger.LogWarning(ex, "Malformed OTLP/HTTP compressed body");
            return BadRequest("Malformed compressed body");
        }

        if (body.Length == 0)
        {
            return BadRequest("Empty request body");
        }

        TRequest request;
        try
        {
            request = format == PayloadFormat.Protobuf
                ? parser.ParseFrom(body)
                : JsonParser.Default.Parse<TRequest>(System.Text.Encoding.UTF8.GetString(body));
        }
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogWarning(ex, "Malformed OTLP protobuf payload");
            return BadRequest("Malformed protobuf payload");
        }
        catch (InvalidJsonException ex)
        {
            _logger.LogWarning(ex, "Malformed OTLP JSON payload");
            return BadRequest("Malformed JSON payload");
        }

        ingest(request);

        var response = responseFactory();
        return format == PayloadFormat.Protobuf
            ? File(response.ToByteArray(), ProtobufContentType)
            : Content(JsonFormatter.Default.Format(response), JsonContentType);
    }

    private async Task<byte[]> ReadBodyAsync()
    {
        var contentEncoding = Request.Headers.ContentEncoding.ToString();
        Stream source = Request.Body;

        if (contentEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase))
        {
            source = new GZipStream(Request.Body, CompressionMode.Decompress);
        }
        else if (contentEncoding.Contains("deflate", StringComparison.OrdinalIgnoreCase))
        {
            source = new DeflateStream(Request.Body, CompressionMode.Decompress);
        }

        await using (source)
        {
            using var ms = new MemoryStream();
            await source.CopyToAsync(ms);
            return ms.ToArray();
        }
    }

    private static PayloadFormat? DetectFormat(string contentType)
    {
        if (contentType.StartsWith(ProtobufContentType, StringComparison.OrdinalIgnoreCase))
            return PayloadFormat.Protobuf;
        if (contentType.StartsWith(JsonContentType, StringComparison.OrdinalIgnoreCase))
            return PayloadFormat.Json;
        return null;
    }

    private enum PayloadFormat
    {
        Protobuf,
        Json
    }
}
