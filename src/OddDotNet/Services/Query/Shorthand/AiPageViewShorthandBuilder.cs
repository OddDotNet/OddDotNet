using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.AppInsights.V1.PageView;

using static OddDotNet.Services.Query.Shorthand.ShorthandBuildingHelpers;

namespace OddDotNet.Services.Query.Shorthand;

public class AiPageViewShorthandBuilder : IShorthandFilterBuilder<PageViewQueryRequest>
{
    private const string Signal = "appinsights/pageviews";

    public PageViewQueryRequest Build(IQueryCollection query)
    {
        var request = new PageViewQueryRequest
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
                case "id":           pf.Id = Eq(value); break;
                case "name":         pf.Name = Eq(value); break;
                case "url":          pf.Url = Eq(value); break;
                case "referrer_uri": pf.ReferrerUri = Eq(value); break;
                default: RejectUnknown(name, Signal); break;
            }
            request.Filters.Add(new Where { Property = pf });
        }

        return request;
    }
}
