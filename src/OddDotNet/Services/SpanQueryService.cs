using Grpc.Core;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Services;

public class SpanQueryService : OddDotNet.Proto.Trace.V1.SpanQueryService.SpanQueryServiceBase
{
    private readonly SignalList<FlatSpan> _signals;

    public SpanQueryService(SignalList<FlatSpan> signals)
    {
        _signals = signals;
    }

    public override async Task<SpanQueryResponse> Query(SpanQueryRequest request, ServerCallContext context)
    {
        var response = new SpanQueryResponse();
        await foreach (FlatSpan span in _signals.QueryAsync(request.Take, request.Duration, request.Filters).WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.Spans.Add(span);
        }

        return response;
    }

    public override Task<SpanResetResponse> Reset(SpanResetRequest request, ServerCallContext context)
    {
        _signals.Reset();
        return Task.FromResult<SpanResetResponse>(new());
    }
}