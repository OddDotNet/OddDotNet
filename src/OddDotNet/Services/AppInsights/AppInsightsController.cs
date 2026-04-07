using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OddDotNet.Proto.AppInsights.V1.Availability;
using OddDotNet.Proto.AppInsights.V1.Dependency;
using OddDotNet.Proto.AppInsights.V1.Event;
using OddDotNet.Proto.AppInsights.V1.Exception;
using OddDotNet.Proto.AppInsights.V1.Metric;
using OddDotNet.Proto.AppInsights.V1.PageView;
using OddDotNet.Proto.AppInsights.V1.Request;
using OddDotNet.Proto.AppInsights.V1.Trace;

namespace OddDotNet.Services.AppInsights;

[ApiController]
[Route("v2")]
public class AppInsightsController : ControllerBase
{
    private readonly SignalList<FlatRequest> _requests;
    private readonly SignalList<FlatDependency> _dependencies;
    private readonly SignalList<FlatException> _exceptions;
    private readonly SignalList<FlatTrace> _traces;
    private readonly SignalList<FlatEvent> _events;
    private readonly SignalList<FlatMetric> _metrics;
    private readonly SignalList<FlatPageView> _pageViews;
    private readonly SignalList<FlatAvailability> _availabilities;
    private readonly ILogger<AppInsightsController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AppInsightsController(
        SignalList<FlatRequest> requests,
        SignalList<FlatDependency> dependencies,
        SignalList<FlatException> exceptions,
        SignalList<FlatTrace> traces,
        SignalList<FlatEvent> events,
        SignalList<FlatMetric> metrics,
        SignalList<FlatPageView> pageViews,
        SignalList<FlatAvailability> availabilities,
        ILogger<AppInsightsController> logger)
    {
        _requests = requests;
        _dependencies = dependencies;
        _exceptions = exceptions;
        _traces = traces;
        _events = events;
        _metrics = metrics;
        _pageViews = pageViews;
        _availabilities = availabilities;
        _logger = logger;
    }

    /// <summary>
    /// App Insights telemetry ingestion endpoint
    /// Accepts single JSON object or newline-delimited JSON (NDJSON)
    /// </summary>
    [HttpPost("track")]
    [HttpPost("/v2.1/track")]
    [Consumes("application/json", "application/x-json-stream", "text/plain")]
    public async Task<IActionResult> Track()
    {
        var contentEncoding = Request.Headers.ContentEncoding.ToString();
        var body = await ReadBodyAsync(contentEncoding);

        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest("Empty request body");
        }

        try
        {
            var envelopes = ParseTelemetry(body);
            var processedCount = 0;

            foreach (var envelope in envelopes)
            {
                ProcessTelemetry(envelope);
                processedCount++;
            }

            return Ok(new { itemsReceived = processedCount, itemsAccepted = processedCount });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse telemetry JSON");
            return BadRequest("Invalid JSON format");
        }
    }

    private async Task<string> ReadBodyAsync(string contentEncoding)
    {
        Stream bodyStream = Request.Body;

        // Handle gzip compression
        if (contentEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase))
        {
            await using var gzipStream = new GZipStream(Request.Body, CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream);
            return await reader.ReadToEndAsync();
        }

        // Handle deflate compression
        if (contentEncoding.Contains("deflate", StringComparison.OrdinalIgnoreCase))
        {
            await using var deflateStream = new DeflateStream(Request.Body, CompressionMode.Decompress);
            using var reader = new StreamReader(deflateStream);
            return await reader.ReadToEndAsync();
        }

        // No compression - read directly
        using var plainReader = new StreamReader(bodyStream);
        return await plainReader.ReadToEndAsync();
    }

    private IEnumerable<AppInsightsTelemetryEnvelope> ParseTelemetry(string body)
    {
        body = body.Trim();

        // Check if it's an array
        if (body.StartsWith('['))
        {
            var envelopes = JsonSerializer.Deserialize<List<AppInsightsTelemetryEnvelope>>(body, JsonOptions);
            return envelopes ?? [];
        }

        // Check if it's NDJSON (newline-delimited JSON)
        if (body.Contains('\n'))
        {
            var results = new List<AppInsightsTelemetryEnvelope>();
            var lines = body.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var envelope = JsonSerializer.Deserialize<AppInsightsTelemetryEnvelope>(line, JsonOptions);
                if (envelope != null)
                {
                    results.Add(envelope);
                }
            }
            
            return results;
        }

        // Single JSON object
        var single = JsonSerializer.Deserialize<AppInsightsTelemetryEnvelope>(body, JsonOptions);
        return single != null ? [single] : [];
    }

    private void ProcessTelemetry(AppInsightsTelemetryEnvelope envelope)
    {
        var baseType = envelope.Data?.BaseType ?? string.Empty;

        switch (baseType)
        {
            case "RequestData":
                _requests.Add(AppInsightsTelemetryConverter.ToFlatRequest(envelope));
                break;

            case "RemoteDependencyData":
                _dependencies.Add(AppInsightsTelemetryConverter.ToFlatDependency(envelope));
                break;

            case "ExceptionData":
                _exceptions.Add(AppInsightsTelemetryConverter.ToFlatException(envelope));
                break;

            case "MessageData":
                _traces.Add(AppInsightsTelemetryConverter.ToFlatTrace(envelope));
                break;

            case "EventData":
                _events.Add(AppInsightsTelemetryConverter.ToFlatEvent(envelope));
                break;

            case "MetricData":
                foreach (var metric in AppInsightsTelemetryConverter.ToFlatMetrics(envelope))
                {
                    _metrics.Add(metric);
                }
                break;

            case "PageViewData":
                _pageViews.Add(AppInsightsTelemetryConverter.ToFlatPageView(envelope));
                break;

            case "AvailabilityData":
                _availabilities.Add(AppInsightsTelemetryConverter.ToFlatAvailability(envelope));
                break;

            default:
                _logger.LogWarning("Unknown telemetry type: {BaseType}", baseType);
                break;
        }
    }

    // ===== Query Endpoints =====

    /// <summary>
    /// Get summary of all telemetry counts
    /// </summary>
    [HttpGet("/appinsights")]
    public IActionResult GetSummary()
    {
        return Ok(new
        {
            requests = _requests.Count,
            dependencies = _dependencies.Count,
            exceptions = _exceptions.Count,
            traces = _traces.Count,
            events = _events.Count,
            metrics = _metrics.Count,
            pageViews = _pageViews.Count,
            availability = _availabilities.Count
        });
    }

    /// <summary>
    /// Get all requests
    /// </summary>
    [HttpGet("/appinsights/requests")]
    public IActionResult GetRequests() => Ok(_requests.GetAll());

    /// <summary>
    /// Get all dependencies
    /// </summary>
    [HttpGet("/appinsights/dependencies")]
    public IActionResult GetDependencies() => Ok(_dependencies.GetAll());

    /// <summary>
    /// Get all exceptions
    /// </summary>
    [HttpGet("/appinsights/exceptions")]
    public IActionResult GetExceptions() => Ok(_exceptions.GetAll());

    /// <summary>
    /// Get all traces
    /// </summary>
    [HttpGet("/appinsights/traces")]
    public IActionResult GetTraces() => Ok(_traces.GetAll());

    /// <summary>
    /// Get all events
    /// </summary>
    [HttpGet("/appinsights/events")]
    public IActionResult GetEvents() => Ok(_events.GetAll());

    /// <summary>
    /// Get all metrics
    /// </summary>
    [HttpGet("/appinsights/metrics")]
    public IActionResult GetMetrics() => Ok(_metrics.GetAll());

    /// <summary>
    /// Get all page views
    /// </summary>
    [HttpGet("/appinsights/pageviews")]
    public IActionResult GetPageViews() => Ok(_pageViews.GetAll());

    /// <summary>
    /// Get all availability results
    /// </summary>
    [HttpGet("/appinsights/availability")]
    public IActionResult GetAvailability() => Ok(_availabilities.GetAll());

    /// <summary>
    /// Reset all App Insights telemetry
    /// </summary>
    [HttpDelete("/appinsights/reset")]
    public IActionResult Reset()
    {
        _requests.Reset();
        _dependencies.Reset();
        _exceptions.Reset();
        _traces.Reset();
        _events.Reset();
        _metrics.Reset();
        _pageViews.Reset();
        _availabilities.Reset();
        return Ok();
    }
}
