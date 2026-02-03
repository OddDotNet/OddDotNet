using Grpc.Core;
using OddDotNet.Proto.AppInsights.V1;

namespace OddDotNet.Services.AppInsights;

public class PageViewQueryService : Proto.AppInsights.V1.PageViewQueryService.PageViewQueryServiceBase
{
    private readonly SignalList<FlatPageView> _signals;

    public PageViewQueryService(SignalList<FlatPageView> signals)
    {
        _signals = signals;
    }

    public override async Task<PageViewQueryResponse> Query(PageViewQueryRequest request, ServerCallContext context)
    {
        var response = new PageViewQueryResponse();
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.PageViews.Add(signal);
        }
        return response;
    }

    public override async Task StreamQuery(PageViewQueryRequest request, IServerStreamWriter<FlatPageView> responseStream, ServerCallContext context)
    {
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            await responseStream.WriteAsync(signal);
        }
    }

    public override Task<PageViewResetResponse> Reset(PageViewResetRequest request, ServerCallContext context)
    {
        _signals.Reset();
        return Task.FromResult(new PageViewResetResponse());
    }
}
