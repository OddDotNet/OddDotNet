using Grpc.Core;
using OddDotNet.Proto.AppInsights.V1;

namespace OddDotNet.Services.AppInsights;

public class AvailabilityQueryService : Proto.AppInsights.V1.AvailabilityQueryService.AvailabilityQueryServiceBase
{
    private readonly SignalList<FlatAvailability> _signals;

    public AvailabilityQueryService(SignalList<FlatAvailability> signals)
    {
        _signals = signals;
    }

    public override async Task<AvailabilityQueryResponse> Query(AvailabilityQueryRequest request, ServerCallContext context)
    {
        var response = new AvailabilityQueryResponse();
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.Availabilities.Add(signal);
        }
        return response;
    }

    public override async Task StreamQuery(AvailabilityQueryRequest request, IServerStreamWriter<FlatAvailability> responseStream, ServerCallContext context)
    {
        await foreach (var signal in _signals.QueryAsync(request.Take, request.Duration, request.Filters)
                           .WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            await responseStream.WriteAsync(signal);
        }
    }

    public override Task<AvailabilityResetResponse> Reset(AvailabilityResetRequest request, ServerCallContext context)
    {
        _signals.Reset();
        return Task.FromResult(new AvailabilityResetResponse());
    }
}
