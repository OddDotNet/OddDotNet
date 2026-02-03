using Grpc.Core;
using OddDotNet.Proto.AppInsights.V1;

namespace OddDotNet.Services.AppInsights;

public class MetricQueryService : Proto.AppInsights.V1.MetricQueryService.MetricQueryServiceBase
{
    private readonly SignalList<FlatMetric> _signals;

    public MetricQueryService(SignalList<FlatMetric> signals)
    {
        _signals = signals;
    }

    public override async Task<MetricQueryResponse> Query(MetricQueryRequest request, ServerCallContext context)
    {
        var response = new MetricQueryResponse();
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.Metrics.Add(signal);
        }
        return response;
    }

    public override async Task StreamQuery(MetricQueryRequest request, IServerStreamWriter<FlatMetric> responseStream, ServerCallContext context)
    {
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            await responseStream.WriteAsync(signal);
        }
    }

    public override Task<MetricResetResponse> Reset(MetricResetRequest request, ServerCallContext context)
    {
        _signals.Reset();
        return Task.FromResult(new MetricResetResponse());
    }
}
