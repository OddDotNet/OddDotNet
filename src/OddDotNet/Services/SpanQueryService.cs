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
        var result = await _spans.QueryAsync(request, context.CancellationToken);
        
        var response = new SpanQueryResponse();
        
        response.Spans.AddRange(result);

        return response;
    }
}