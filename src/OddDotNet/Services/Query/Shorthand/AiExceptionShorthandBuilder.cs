using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.AppInsights.V1.Exception;

using static OddDotNet.Services.Query.Shorthand.ShorthandBuildingHelpers;

namespace OddDotNet.Services.Query.Shorthand;

public class AiExceptionShorthandBuilder : IShorthandFilterBuilder<ExceptionQueryRequest>
{
    private const string Signal = "appinsights/exceptions";

    public ExceptionQueryRequest Build(IQueryCollection query)
    {
        var request = new ExceptionQueryRequest
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
                case "id":         pf.Id = Eq(value); break;
                case "problem_id": pf.ProblemId = Eq(value); break;
                default: RejectUnknown(name, Signal); break;
            }
            request.Filters.Add(new Where { Property = pf });
        }

        return request;
    }
}
