using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.Query.Shorthand;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class ShorthandBuilderUnitTests
{
    private static IQueryCollection Q(params (string k, string v)[] pairs) =>
        new QueryCollection(pairs.ToDictionary(p => p.k, p => new StringValues(p.v)));

    [Fact]
    public void SpanBuilder_WithName_ProducesEqualsFilter()
    {
        var req = new SpanShorthandBuilder().Build(Q(("name", "checkout")));

        Assert.Single(req.Filters);
        Assert.Equal("checkout", req.Filters[0].Property.Name.Compare);
        Assert.Equal(StringCompareAsType.Equals, req.Filters[0].Property.Name.CompareAs);
    }

    [Fact]
    public void SpanBuilder_WithAttr_PutsDotsIntoKey()
    {
        var req = new SpanShorthandBuilder().Build(Q(("attr.service.name", "svc-a")));

        var attrWhere = req.Filters.Single(w => w.Property.Attributes != null);
        var kv = attrWhere.Property.Attributes.Values[0];
        Assert.Equal("service.name", kv.Key);
        Assert.Equal("svc-a", kv.Value.StringValue.Compare);
    }

    [Fact]
    public void SpanBuilder_WithBothNameAndAttr_ProducesTwoAndedWheres()
    {
        var req = new SpanShorthandBuilder().Build(Q(("name", "checkout"), ("attr.env", "prod")));

        Assert.Equal(2, req.Filters.Count);
        Assert.Contains(req.Filters, w => w.Property.Name != null && w.Property.Name.Compare == "checkout");
        Assert.Contains(req.Filters, w => w.Property.Attributes != null);
    }

    [Fact]
    public void SpanBuilder_UnknownField_Throws()
    {
        var ex = Assert.Throws<ShorthandParseException>(() => new SpanShorthandBuilder().Build(Q(("favorite_color", "blue"))));
        Assert.Contains("favorite_color", ex.Message);
    }

    [Fact]
    public void SpanBuilder_MalformedTraceIdHex_Throws()
    {
        Assert.Throws<ShorthandParseException>(() => new SpanShorthandBuilder().Build(Q(("trace_id", "not-hex-ZZ"))));
    }

    [Fact]
    public void MetricsBuilder_AttrParam_Rejected()
    {
        Assert.Throws<ShorthandParseException>(() => new MetricShorthandBuilder().Build(Q(("attr.k", "v"))));
    }

    [Fact]
    public void AiRequestBuilder_WithId_ProducesFilter()
    {
        var req = new AiRequestShorthandBuilder().Build(Q(("id", "req-123")));
        Assert.Equal("req-123", req.Filters[0].Property.Id.Compare);
    }

    [Fact]
    public void AllBuilders_HonorTakeParam()
    {
        Assert.Equal(Take.ValueOneofCase.TakeAll, new SpanShorthandBuilder().Build(Q(("take", "all"))).Take.ValueCase);
        Assert.Equal(Take.ValueOneofCase.TakeAll, new LogShorthandBuilder().Build(Q(("take", "all"))).Take.ValueCase);
        Assert.Equal(Take.ValueOneofCase.TakeAll, new AiEventShorthandBuilder().Build(Q(("take", "all"))).Take.ValueCase);
    }
}
