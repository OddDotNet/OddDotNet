using Grpc.Core;
using OddDotNet.Proto.Metrics.V1;
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
        foreach (var resourceMetric in request.ResourceMetrics)
        {
            foreach (var scopeMetric in resourceMetric.ScopeMetrics)
            {
                foreach (var metric in scopeMetric.Metrics)
                {
                    var flatMetric = new FlatMetric
                    {
                        Metric = metric,
                        InstrumentationScope = scopeMetric.Scope,
                        Resource = resourceMetric.Resource,
                        ResourceSchemaUrl = resourceMetric.SchemaUrl,
                        InstrumentationScopeSchemaUrl = resourceMetric.SchemaUrl
                    };
                    _signals.Add(flatMetric);
                }
            }
        }

        return Task.FromResult(new ExportMetricsServiceResponse());
    }
}