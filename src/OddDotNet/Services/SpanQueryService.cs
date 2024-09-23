using Grpc.Core;

namespace OddDotNet.Services;

public class SpanQueryService : OddDotNet.SpanQueryService.SpanQueryServiceBase
{
    private readonly ISignalList<Span> _spans;

    public SpanQueryService(ISignalList<Span> spans)
    {
        _spans = spans;
    }

    public override async Task<SpanQueryResponse> Query(SpanQueryRequest request, ServerCallContext context)
    {
        var response = new SpanQueryResponse();
        await foreach (Span span in _spans.QueryAsync(request).WithCancellation(context.CancellationToken).ConfigureAwait(false))
            response.Spans.Add(span);

        return response;
    }

    public override Task<SpanResetResponse> Reset(SpanResetRequest request, ServerCallContext context)
    {
        _spans.Reset(request);
        return Task.FromResult<SpanResetResponse>(new());
    }
}