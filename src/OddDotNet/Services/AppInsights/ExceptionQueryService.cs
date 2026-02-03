using Grpc.Core;
using OddDotNet.Proto.AppInsights.V1;

namespace OddDotNet.Services.AppInsights;

public class ExceptionQueryService : Proto.AppInsights.V1.ExceptionQueryService.ExceptionQueryServiceBase
{
    private readonly SignalList<FlatException> _signals;

    public ExceptionQueryService(SignalList<FlatException> signals)
    {
        _signals = signals;
    }

    public override async Task<ExceptionQueryResponse> Query(ExceptionQueryRequest request, ServerCallContext context)
    {
        var response = new ExceptionQueryResponse();
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.Exceptions.Add(signal);
        }
        return response;
    }

    public override async Task StreamQuery(ExceptionQueryRequest request, IServerStreamWriter<FlatException> responseStream, ServerCallContext context)
    {
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            await responseStream.WriteAsync(signal);
        }
    }

    public override Task<ExceptionResetResponse> Reset(ExceptionResetRequest request, ServerCallContext context)
    {
        _signals.Reset();
        return Task.FromResult(new ExceptionResetResponse());
    }
}
