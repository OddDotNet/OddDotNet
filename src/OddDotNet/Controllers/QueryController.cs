using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OddDotNet.Proto.Logs.V1;

namespace OddDotNet.Controllers;

[ApiController]
[Route("v1/query")]
public class QueryController : ControllerBase
{
    private readonly SignalList<FlatLog> _logs;

    public QueryController(SignalList<FlatLog> logs)
    {
        _logs = logs;
    }

    [HttpPost("logs")]
    [ProducesResponseType<LogQueryResponse>(200, "application/json", "application/x-protobuf")]
    [ProducesResponseType<ProblemDetails>(400, "application/json", "application/x-protobuf")]
    [Consumes("application/json", "application/x-protobuf")]
    public async Task<Results<BadRequest, Ok<LogQueryResponse>>> QueryLogs([FromBody] LogQueryRequest request,
        CancellationToken cancellationToken)
    {
        var response = new LogQueryResponse();
        await foreach (FlatLog log in _logs.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            response.Logs.Add(log);
        }

        return TypedResults.Ok(response);
    }
}