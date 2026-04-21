using Grpc.Core;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Services.Otlp;
using OpenTelemetry.Proto.Collector.Metrics.V1;

namespace OddDotNet.Services;

public class MetricsService : OpenTelemetry.Proto.Collector.Metrics.V1.MetricsService.MetricsServiceBase
{
    private readonly SignalList<FlatMetric> _signals;

    public MetricsService(SignalList<FlatMetric> signals)
    {
        _signals = signals;
    }

    public override Task<ExportMetricsServiceResponse> Export(ExportMetricsServiceRequest request, ServerCallContext context)
    {
        OtlpFlattener.Flatten(request, _signals);
        return Task.FromResult(new ExportMetricsServiceResponse());
    }
}