using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Logs.V1;

using static OddDotNet.Services.Query.Shorthand.ShorthandBuildingHelpers;

namespace OddDotNet.Services.Query.Shorthand;

public class LogShorthandBuilder : IShorthandFilterBuilder<LogQueryRequest>
{
    private const string Signal = "logs";

    public LogQueryRequest Build(IQueryCollection query)
    {
        var request = new LogQueryRequest
        {
            Take = ShorthandParamParser.ParseTake(query),
            Duration = ShorthandParamParser.ParseWaitMs(query)
        };

        KeyValueListProperty? attrs = null;

        foreach (var (name, values) in query)
        {
            if (ShorthandParamParser.IsReservedParam(name)) continue;
            if (values.Count == 0) continue;
            var value = values.ToString();

            if (ShorthandParamParser.TryGetAttributeKey(name, out var attrKey))
            {
                attrs ??= new KeyValueListProperty();
                attrs.Values.Add(BuildAttr(attrKey, value));
                continue;
            }

            var pf = new PropertyFilter();
            switch (name)
            {
                case "severity_text": pf.SeverityText = Eq(value); break;
                case "trace_id":      pf.TraceId = EqBytes(name, value); break;
                case "span_id":       pf.SpanId = EqBytes(name, value); break;
                case "body":          pf.Body = new AnyValueProperty { StringValue = Eq(value) }; break;
                default: RejectUnknown(name, Signal); break;
            }
            request.Filters.Add(new Where { Property = pf });
        }

        if (attrs is not null)
        {
            request.Filters.Add(new Where { Property = new PropertyFilter { Attributes = attrs } });
        }

        return request;
    }
}
