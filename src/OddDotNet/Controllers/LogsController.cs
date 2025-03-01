using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OddDotNet.Proto.Logs.V1;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace OddDotNet.Controllers;

[ApiController]
[Route("v1/logs")]
public class LogsController : ControllerBase
{
    private readonly SignalList<FlatLog> _signals;

    public LogsController(SignalList<FlatLog> signals)
    {
        _signals = signals;
    }
    
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    public Results<BadRequest, Ok<ExportLogsServiceResponse>> ExportAsync([FromBody] ExportLogsServiceRequest request)
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

        return TypedResults.Ok(new ExportLogsServiceResponse());
    }
}