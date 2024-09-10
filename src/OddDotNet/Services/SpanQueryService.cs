using Grpc.Core;

namespace OddDotNet.Services;

public class SpanQueryService : OddDotNet.SpanQueryService.SpanQueryServiceBase
{
    public override Task<SpanQueryResponse> Query(SpanQueryRequest request, ServerCallContext context)
    {
        return base.Query(request, context);
    }
}