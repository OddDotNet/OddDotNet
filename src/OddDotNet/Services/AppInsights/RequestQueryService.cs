using Grpc.Core;
using OddDotNet.Proto.AppInsights.V1.Request;

namespace OddDotNet.Services.AppInsights;

public class RequestQueryService : Proto.AppInsights.V1.Request.RequestQueryService.RequestQueryServiceBase
{
    private readonly SignalList<FlatRequest> _signals;

    public RequestQueryService(SignalList<FlatRequest> signals)
    {
        _signals = signals;
    }

    public override async Task<RequestQueryResponse> Query(RequestQueryRequest request, ServerCallContext context)
    {
        var response = new RequestQueryResponse();
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.Requests.Add(signal);
        }
        return response;
    }

    public override async Task StreamQuery(RequestQueryRequest request, IServerStreamWriter<FlatRequest> responseStream, ServerCallContext context)
    {
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            await responseStream.WriteAsync(signal);
        }
    }

    public override Task<RequestResetResponse> Reset(RequestResetRequest request, ServerCallContext context)
    {
        _signals.Reset();
        return Task.FromResult(new RequestResetResponse());
    }
}
