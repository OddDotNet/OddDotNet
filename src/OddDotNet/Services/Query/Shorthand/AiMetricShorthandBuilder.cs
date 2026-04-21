using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.AppInsights.V1.Metric;

using static OddDotNet.Services.Query.Shorthand.ShorthandBuildingHelpers;

namespace OddDotNet.Services.Query.Shorthand;

public class AiMetricShorthandBuilder : IShorthandFilterBuilder<MetricQueryRequest>
{
    private const string Signal = "appinsights/metrics";

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

            var pf = new PropertyFilter();
            switch (name)
            {
                case "name":             pf.Name = Eq(value); break;
                case "metric_namespace": pf.MetricNamespace = Eq(value); break;
                default: RejectUnknown(name, Signal); break;
            }
            request.Filters.Add(new Where { Property = pf });
        }

        return request;
    }
}
