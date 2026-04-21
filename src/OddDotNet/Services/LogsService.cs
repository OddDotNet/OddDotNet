using Grpc.Core;
using OddDotNet.Proto.Logs.V1;
using OddDotNet.Services.Otlp;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace OddDotNet.Services;

public class LogsService : OpenTelemetry.Proto.Collector.Logs.V1.LogsService.LogsServiceBase
{
    private readonly SignalList<FlatLog> _signals;

    public LogsService(SignalList<FlatLog> signals)
    {
        _signals = signals;
    }

    public override Task<ExportLogsServiceResponse> Export(ExportLogsServiceRequest request, ServerCallContext context)
    {
        OtlpFlattener.Flatten(request, _signals);
        return Task.FromResult(new ExportLogsServiceResponse());
    }
}