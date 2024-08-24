using Grpc.Core;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace OddDotNet;

public class LogsService : OpenTelemetry.Proto.Collector.Logs.V1.LogsService.LogsServiceBase
{
    public override Task<ExportLogsServiceResponse> Export(ExportLogsServiceRequest request, ServerCallContext context)
    {
        Console.WriteLine("***** Received a log");
        return base.Export(request, context);
    }
}