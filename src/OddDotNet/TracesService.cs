using Grpc.Core;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace OddDotNet;

public class TracesService : TraceService.TraceServiceBase
{
    private readonly ILogger<TracesService> _logger;

    public TracesService(ILogger<TracesService> logger)
    {
        _logger = logger;
    }

    public override Task<ExportTraceServiceResponse> Export(ExportTraceServiceRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received a trace");
        return base.Export(request, context);
    }
}