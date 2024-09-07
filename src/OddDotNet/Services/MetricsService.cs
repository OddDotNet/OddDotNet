using Grpc.Core;
using OpenTelemetry.Proto.Collector.Metrics.V1;

namespace OddDotNet.Services;

public class MetricsService : OpenTelemetry.Proto.Collector.Metrics.V1.MetricsService.MetricsServiceBase
{
    public override Task<ExportMetricsServiceResponse> Export(ExportMetricsServiceRequest request, ServerCallContext context)
    {
        return base.Export(request, context);
    }
}