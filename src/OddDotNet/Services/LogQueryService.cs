using Grpc.Core;
using OddDotNet.Proto.Logs.V1;

namespace OddDotNet.Services;

public class LogQueryService : Proto.Logs.V1.LogQueryService.LogQueryServiceBase
{
    private readonly SignalList<FlatLog> _signals;

    public LogQueryService(SignalList<FlatLog> signals)
    {
        _signals = signals;
    }

    public override async Task<LogQueryResponse> Query(LogQueryRequest request, ServerCallContext context)
    {
        var response = new LogQueryResponse();
        await foreach (FlatLog log in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken).ConfigureAwait(false))
        {
            response.Logs.Add(log);
        }

        return response;
    }

    public override async Task StreamQuery(LogQueryRequest request, IServerStreamWriter<FlatLog> responseStream, ServerCallContext context)
    {
        await foreach (FlatLog log in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken).ConfigureAwait(false))
        {
            await responseStream.WriteAsync(log);
        }
    }
}