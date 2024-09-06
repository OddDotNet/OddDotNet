using Grpc.Core;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace OddDotNet;

public class LogsService : OpenTelemetry.Proto.Collector.Logs.V1.LogsService.LogsServiceBase
{
    private readonly IOpenTelemetryTestHarness _testHarness;

    public LogsService(IOpenTelemetryTestHarness testHarness)
    {
        _testHarness = testHarness;
    }

    public override Task<ExportLogsServiceResponse> Export(ExportLogsServiceRequest request, ServerCallContext context)
    {
        foreach (var whatever in request.ResourceLogs)
        {
            foreach (var whatevs in whatever.ScopeLogs)
            {
                foreach (var what in whatevs.LogRecords)
                {
                    _testHarness.Logs.Add(what);
                }
            }
        }

        return Task.FromResult(new ExportLogsServiceResponse());
    }
}