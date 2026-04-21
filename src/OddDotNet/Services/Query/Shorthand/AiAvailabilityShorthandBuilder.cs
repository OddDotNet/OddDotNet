using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.AppInsights.V1.Availability;

using static OddDotNet.Services.Query.Shorthand.ShorthandBuildingHelpers;

namespace OddDotNet.Services.Query.Shorthand;

public class AiAvailabilityShorthandBuilder : IShorthandFilterBuilder<AvailabilityQueryRequest>
{
    private const string Signal = "appinsights/availability";

    public AvailabilityQueryRequest Build(IQueryCollection query)
    {
        var request = new AvailabilityQueryRequest
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
                case "run_location": pf.RunLocation = Eq(value); break;
                case "message":      pf.Message = Eq(value); break;
                default: RejectUnknown(name, Signal); break;
            }
            request.Filters.Add(new Where { Property = pf });
        }

        return request;
    }
}
