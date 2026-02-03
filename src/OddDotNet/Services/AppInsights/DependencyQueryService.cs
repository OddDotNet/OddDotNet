using Grpc.Core;
using OddDotNet.Proto.AppInsights.V1;

namespace OddDotNet.Services.AppInsights;

public class DependencyQueryService : Proto.AppInsights.V1.DependencyQueryService.DependencyQueryServiceBase
{
    private readonly SignalList<FlatDependency> _signals;

    public DependencyQueryService(SignalList<FlatDependency> signals)
    {
        _signals = signals;
    }

    public override async Task<DependencyQueryResponse> Query(DependencyQueryRequest request, ServerCallContext context)
    {
        var response = new DependencyQueryResponse();
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.Dependencies.Add(signal);
        }
        return response;
    }

    public override async Task StreamQuery(DependencyQueryRequest request, IServerStreamWriter<FlatDependency> responseStream, ServerCallContext context)
    {
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            await responseStream.WriteAsync(signal);
        }
    }

    public override Task<DependencyResetResponse> Reset(DependencyResetRequest request, ServerCallContext context)
    {
        _signals.Reset();
        return Task.FromResult(new DependencyResetResponse());
    }
}
