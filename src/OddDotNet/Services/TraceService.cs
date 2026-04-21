using Grpc.Core;
using OddDotNet.Proto.Trace.V1;
using OddDotNet.Services.Otlp;
using OpenTelemetry.Proto.Collector.Trace.V1;

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
        OtlpFlattener.Flatten(request, _spans);
        return Task.FromResult(new ExportTraceServiceResponse());
    }
}