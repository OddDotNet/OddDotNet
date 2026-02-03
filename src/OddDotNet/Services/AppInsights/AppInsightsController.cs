using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OddDotNet.Proto.AppInsights.V1;

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
    [Consumes("application/json", "text/plain")]
    public async Task<IActionResult> Track()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

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

            _logger.LogDebug("Processed {Count} telemetry items", processedCount);
            return Ok(new { itemsReceived = processedCount, itemsAccepted = processedCount });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse telemetry JSON");
            return BadRequest("Invalid JSON format");
        }
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
}
