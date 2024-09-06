using Grpc.Core;
using OpenTelemetry.Proto.Collector.Metrics.V1;

namespace OddDotNet;

public class MetricsService : OpenTelemetry.Proto.Collector.Metrics.V1.MetricsService.MetricsServiceBase
{
    private readonly IOpenTelemetryTestHarness _testHarness;

    public MetricsService(IOpenTelemetryTestHarness testHarness)
    {
        _testHarness = testHarness;
    }

    public override Task<ExportMetricsServiceResponse> Export(ExportMetricsServiceRequest request,
        ServerCallContext context)
    {
        
        foreach (var a in request.ResourceMetrics)
        {
            foreach (var b in a.ScopeMetrics)
            {
                foreach (var c in b.Metrics)
                {
                    _testHarness.Metrics.Add(c);
                }
            }
        }

        return Task.FromResult(new ExportMetricsServiceResponse());
    }
}