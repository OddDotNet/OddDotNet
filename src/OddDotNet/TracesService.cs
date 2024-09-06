using Grpc.Core;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace OddDotNet;

public class TracesService : TraceService.TraceServiceBase
{
    private readonly ILogger<TracesService> _logger;
    private readonly IOpenTelemetryTestHarness _testHarness;

    public TracesService(ILogger<TracesService> logger, IOpenTelemetryTestHarness testHarness)
    {
        _logger = logger;
        _testHarness = testHarness;
    }

    public override Task<ExportTraceServiceResponse> Export(ExportTraceServiceRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received a trace");
        foreach (var span in request.ResourceSpans)
        {
            
            foreach (var scopeSpan in span.ScopeSpans)
            {
                foreach (var whatever in scopeSpan.Spans)
                {
                    _testHarness.Traces.Add(whatever);
                    _logger.LogInformation("Name of span: {name}", whatever.Name);
                }
            }
        }
        
        return Task.FromResult(new ExportTraceServiceResponse());
    }
}