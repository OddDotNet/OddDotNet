using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.AppInsights.V1.Trace;

using static OddDotNet.Services.Query.Shorthand.ShorthandBuildingHelpers;

namespace OddDotNet.Services.Query.Shorthand;

public class AiTraceShorthandBuilder : IShorthandFilterBuilder<TraceQueryRequest>
{
    private const string Signal = "appinsights/traces";

    public TraceQueryRequest Build(IQueryCollection query)
    {
        var request = new TraceQueryRequest
        {
            Take = ShorthandParamParser.ParseTake(query),
            Duration = ShorthandParamParser.ParseWaitMs(query)
        };

        foreach (var (name, values) in query)
        {
            if (ShorthandParamParser.IsReservedParam(name)) continue;
            if (values.Count == 0) continue;
            var value = values.ToString();

            var pf = new PropertyFilter();
            switch (name)
            {
                case "message": pf.Message = Eq(value); break;
                default: RejectUnknown(name, Signal); break;
            }
            request.Filters.Add(new Where { Property = pf });
        }

        return request;
    }
}
