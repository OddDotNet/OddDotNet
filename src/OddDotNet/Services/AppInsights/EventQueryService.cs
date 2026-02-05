using Grpc.Core;
using OddDotNet.Proto.AppInsights.V1.Event;

namespace OddDotNet.Services.AppInsights;

public class EventQueryService : Proto.AppInsights.V1.Event.EventQueryService.EventQueryServiceBase
{
    private readonly SignalList<FlatEvent> _signals;

    public EventQueryService(SignalList<FlatEvent> signals)
    {
        _signals = signals;
    }

    public override async Task<EventQueryResponse> Query(EventQueryRequest request, ServerCallContext context)
    {
        var response = new EventQueryResponse();
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.Events.Add(signal);
        }
        return response;
    }

    public override async Task StreamQuery(EventQueryRequest request, IServerStreamWriter<FlatEvent> responseStream, ServerCallContext context)
    {
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            await responseStream.WriteAsync(signal);
        }
    }

    public override Task<EventResetResponse> Reset(EventResetRequest request, ServerCallContext context)
    {
        _signals.Reset();
        return Task.FromResult(new EventResetResponse());
    }
}
