using Grpc.Core;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace OddDotNet.Services;

public class TraceService : OpenTelemetry.Proto.Collector.Trace.V1.TraceService.TraceServiceBase
{
    public override Task<ExportTraceServiceResponse> Export(ExportTraceServiceRequest request, ServerCallContext context)
    {
        return base.Export(request, context);
    }
}