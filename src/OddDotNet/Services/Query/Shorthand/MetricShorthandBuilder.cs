using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.Metrics.V1;

using static OddDotNet.Services.Query.Shorthand.ShorthandBuildingHelpers;

namespace OddDotNet.Services.Query.Shorthand;

public class MetricShorthandBuilder : IShorthandFilterBuilder<MetricQueryRequest>
{
    private const string Signal = "metrics";

    public MetricQueryRequest Build(IQueryCollection query)
    {
        var request = new MetricQueryRequest
        {
            Take = ShorthandParamParser.ParseTake(query),
            Duration = ShorthandParamParser.ParseWaitMs(query)
        };

        foreach (var (name, values) in query)
        {
            if (ShorthandParamParser.IsReservedParam(name)) continue;
            if (values.Count == 0) continue;
            var value = values.ToString();

            if (ShorthandParamParser.TryGetAttributeKey(name, out _))
            {
                throw new ShorthandParseException(
                    "attr.* filters are not supported on /query/v1/metrics in phase 2");
            }

            var pf = new PropertyFilter();
            switch (name)
            {
                case "name":        pf.Name = Eq(value); break;
                case "description": pf.Description = Eq(value); break;
                case "unit":        pf.Unit = Eq(value); break;
                default: RejectUnknown(name, Signal); break;
            }
            request.Filters.Add(new Where { Property = pf });
        }

        return request;
    }
}
