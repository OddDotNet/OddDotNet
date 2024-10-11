using Grpc.Core;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Services;

public class SpanQueryService : OddDotNet.Proto.Trace.V1.SpanQueryService.SpanQueryServiceBase
{
    private readonly ISignalList<FlatSpan> _spans;

    public SpanQueryService(ISignalList<FlatSpan> spans)
    {
        _spans = spans;
    }

    public override async Task<SpanQueryResponse> Query(SpanQueryRequest request, ServerCallContext context)
    {
        var response = new SpanQueryResponse();
        await foreach (FlatSpan span in _spans.QueryAsync(request).WithCancellation(context.CancellationToken)
                           .ConfigureAwait(false))
        {
            response.Spans.Add(span);
        }

        return response;
    }

    public override Task<SpanResetResponse> Reset(SpanResetRequest request, ServerCallContext context)
    {
        _spans.Reset(request);
        return Task.FromResult<SpanResetResponse>(new());
    }
}