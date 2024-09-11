using Grpc.Core;

namespace OddDotNet.Services;

public class SpanQueryService : OddDotNet.SpanQueryService.SpanQueryServiceBase
{
    private readonly IChannelManager<Span> _channels;
    private readonly ISignalList<Span> _spans;

    public SpanQueryService(IChannelManager<Span> channels, ISignalList<Span> spans)
    {
        _channels = channels;
        _spans = spans;
    }

    public override async Task<SpanQueryResponse> Query(SpanQueryRequest request, ServerCallContext context)
    {
        var result = await _spans.QueryAsync(request, context.CancellationToken);
        
        // TODO what does this response look like?
        return new SpanQueryResponse();
    }
}