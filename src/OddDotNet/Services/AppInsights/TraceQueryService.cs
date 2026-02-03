using Grpc.Core;
using OddDotNet.Proto.AppInsights.V1.Trace;

namespace OddDotNet.Services.AppInsights;

public class TraceQueryService : Proto.AppInsights.V1.Trace.TraceQueryService.TraceQueryServiceBase
{
    private readonly SignalList<FlatTrace> _signals;

    public TraceQueryService(SignalList<FlatTrace> signals)
    {
        _signals = signals;
    }

    public override async Task<TraceQueryResponse> Query(TraceQueryRequest request, ServerCallContext context)
    {
        var response = new TraceQueryResponse();
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.Traces.Add(signal);
        }
        return response;
    }

    public override async Task StreamQuery(TraceQueryRequest request, IServerStreamWriter<FlatTrace> responseStream, ServerCallContext context)
    {
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            await responseStream.WriteAsync(signal);
        }
    }

    public override Task<TraceResetResponse> Reset(TraceResetRequest request, ServerCallContext context)
    {
        _signals.Reset();
        return Task.FromResult(new TraceResetResponse());
    }
}
