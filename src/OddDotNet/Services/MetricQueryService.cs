using Grpc.Core;
using OddDotNet.Proto.Metrics.V1;

namespace OddDotNet.Services;

public class MetricQueryService : OddDotNet.Proto.Metrics.V1.MetricQueryService.MetricQueryServiceBase
{
    private readonly SignalList<FlatMetric> _signals;

    public MetricQueryService(SignalList<FlatMetric> signals)
    {
        _signals = signals;
    }

    public override async Task<MetricQueryResponse> Query(MetricQueryRequest request, ServerCallContext context)
    {
        var response = new MetricQueryResponse();
        await foreach (FlatMetric metric in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken).ConfigureAwait(false))
        {
            response.Metrics.Add(metric);
        }

        return response;
    }

    public override async Task StreamQuery(MetricQueryRequest request, IServerStreamWriter<FlatMetric> responseStream, ServerCallContext context)
    {
        await foreach (FlatMetric metric in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken).ConfigureAwait(false))
        {
            await responseStream.WriteAsync(metric);
        }
    }
}