using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;

using static OddDotNet.Services.Query.Shorthand.ShorthandBuildingHelpers;

namespace OddDotNet.Services.Query.Shorthand;

public class SpanShorthandBuilder : IShorthandFilterBuilder<SpanQueryRequest>
{
    private const string Signal = "spans";

    public SpanQueryRequest Build(IQueryCollection query)
    {
        var request = new SpanQueryRequest
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
                case "name":          pf.Name = Eq(value); break;
                case "trace_state":   pf.TraceState = Eq(value); break;
                case "trace_id":      pf.TraceId = EqBytes(name, value); break;
                case "span_id":       pf.SpanId = EqBytes(name, value); break;
                case "parent_span_id": pf.ParentSpanId = EqBytes(name, value); break;
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
