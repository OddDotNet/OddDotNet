using Grpc.Core;
using OddDotNet.Proto.Logs.V1;
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
        foreach (var resourceLog in request.ResourceLogs)
        {
            foreach (var scopeLog in resourceLog.ScopeLogs)
            {
                foreach (var log in scopeLog.LogRecords)
                {
                    var flatLog = new FlatLog
                    {
                        Log = log,
                        InstrumentationScope = scopeLog.Scope,
                        Resource = resourceLog.Resource,
                        ResourceSchemaUrl = resourceLog.SchemaUrl,
                        InstrumentationScopeSchemaUrl = scopeLog.SchemaUrl
                    };
                    _signals.Add(flatLog);
                }
            }
        }

        return Task.FromResult(new ExportLogsServiceResponse());
    }
}