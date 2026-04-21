using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.AppInsights.V1.Dependency;

using static OddDotNet.Services.Query.Shorthand.ShorthandBuildingHelpers;

namespace OddDotNet.Services.Query.Shorthand;

public class AiDependencyShorthandBuilder : IShorthandFilterBuilder<DependencyQueryRequest>
{
    private const string Signal = "appinsights/dependencies";

    public DependencyQueryRequest Build(IQueryCollection query)
    {
        var request = new DependencyQueryRequest
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
                case "id":          pf.Id = Eq(value); break;
                case "name":        pf.Name = Eq(value); break;
                case "result_code": pf.ResultCode = Eq(value); break;
                case "data":        pf.Data = Eq(value); break;
                case "target":      pf.Target = Eq(value); break;
                case "type":        pf.Type = Eq(value); break;
                default: RejectUnknown(name, Signal); break;
            }
            request.Filters.Add(new Where { Property = pf });
        }

        return request;
    }
}
