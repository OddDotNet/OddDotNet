using OddDotNet.Proto.Logs.V1;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace OddDotNet.Services.Otlp;

public static class OtlpFlattener
{
    public static void Flatten(ExportTraceServiceRequest request, SignalList<FlatSpan> signals)
    {
        foreach (var resourceSpan in request.ResourceSpans)
        {
            foreach (var scopeSpan in resourceSpan.ScopeSpans)
            {
                foreach (var span in scopeSpan.Spans)
                {
                    signals.Add(new FlatSpan
                    {
                        Span = span,
                        InstrumentationScope = scopeSpan.Scope,
                        Resource = resourceSpan.Resource,
                        ResourceSchemaUrl = resourceSpan.SchemaUrl,
                        InstrumentationScopeSchemaUrl = scopeSpan.SchemaUrl
                    });
                }
            }
        }
    }

    public static void Flatten(ExportMetricsServiceRequest request, SignalList<FlatMetric> signals)
    {
        foreach (var resourceMetric in request.ResourceMetrics)
        {
            foreach (var scopeMetric in resourceMetric.ScopeMetrics)
            {
                foreach (var metric in scopeMetric.Metrics)
                {
                    signals.Add(new FlatMetric
                    {
                        Metric = metric,
                        InstrumentationScope = scopeMetric.Scope,
                        Resource = resourceMetric.Resource,
                        ResourceSchemaUrl = resourceMetric.SchemaUrl,
                        InstrumentationScopeSchemaUrl = scopeMetric.SchemaUrl
                    });
                }
            }
        }
    }

    public static void Flatten(ExportLogsServiceRequest request, SignalList<FlatLog> signals)
    {
        foreach (var resourceLog in request.ResourceLogs)
        {
            foreach (var scopeLog in resourceLog.ScopeLogs)
            {
                foreach (var log in scopeLog.LogRecords)
                {
                    signals.Add(new FlatLog
                    {
                        Log = log,
                        InstrumentationScope = scopeLog.Scope,
                        Resource = resourceLog.Resource,
                        ResourceSchemaUrl = resourceLog.SchemaUrl,
                        InstrumentationScopeSchemaUrl = scopeLog.SchemaUrl
                    });
                }
            }
        }
    }
}
