using Grpc.Core;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Services;

public class TraceService : OpenTelemetry.Proto.Collector.Trace.V1.TraceService.TraceServiceBase
{
    private readonly SignalList<FlatSpan> _spans;

    public TraceService(SignalList<FlatSpan> spans)
    {
        _spans = spans;
    }

    public override Task<ExportTraceServiceResponse> Export(ExportTraceServiceRequest request, ServerCallContext context)
    {
        foreach (ResourceSpans resourceSpan in request.ResourceSpans)
        {
            foreach (var scopeSpan in resourceSpan.ScopeSpans)
            {
                foreach (var span in scopeSpan.Spans)
                {
                    var flatSpan = new FlatSpan
                    {
                        Span = span,
                        InstrumentationScope = scopeSpan.Scope,
                        Resource = resourceSpan.Resource,
                        ResourceSchemaUrl = resourceSpan.SchemaUrl,
                        InstrumentationScopeSchemaUrl = scopeSpan.SchemaUrl
                    };
                    _spans.Add(flatSpan);
                }
            }
        }

        return Task.FromResult(new ExportTraceServiceResponse());
    }
}