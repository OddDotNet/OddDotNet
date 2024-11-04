using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Trace.V1;

public class SpanLinkTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public SpanLinkTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnSpansWithMatchingLinkTraceIdProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Link = new LinkFilter
                {
                    TraceId = new ByteStringProperty
                    {
                        CompareAs = ByteStringCompareAsType.Equals,
                        Compare = spanToFind.Links[0].TraceId
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Contains(response.Spans, flatSpan => flatSpan.Span.SpanId == spanToFind.SpanId);
    }
    
    [Fact]
    public async Task ReturnSpansWithMatchingLinkSpanIdProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Link = new LinkFilter
                {
                    SpanId = new ByteStringProperty
                    {
                        CompareAs = ByteStringCompareAsType.Equals,
                        Compare = spanToFind.Links[0].SpanId
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Contains(response.Spans, flatSpan => flatSpan.Span.SpanId == spanToFind.SpanId);
    }
    
    [Fact]
    public async Task ReturnSpansWithMatchingLinkTraceStateProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Link = new LinkFilter
                {
                    TraceState = new StringProperty
                    {
                        CompareAs = StringCompareAsType.Equals,
                        Compare = spanToFind.Links[0].TraceState
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Contains(response.Spans, flatSpan => flatSpan.Span.SpanId == spanToFind.SpanId);
    }
    
    [Fact]
    public async Task ReturnSpansWithMatchingLinkAttributeProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Link = new LinkFilter
                {
                    Attribute = new KeyValueProperty
                    {
                        Key = spanToFind.Links[0].Attributes[0].Key,
                        StringValue = new StringProperty
                        {
                            CompareAs = StringCompareAsType.Equals,
                            Compare = spanToFind.Links[0].Attributes[0].Value.StringValue
                        }
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Contains(response.Spans, flatSpan => flatSpan.Span.SpanId == spanToFind.SpanId);
    }
    
    [Fact]
    public async Task ReturnSpansWithMatchingLinkDroppedAttributesCountProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Link = new LinkFilter
                {
                    DroppedAttributesCount = new UInt32Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = spanToFind.Links[0].DroppedAttributesCount
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Contains(response.Spans, flatSpan => flatSpan.Span.SpanId == spanToFind.SpanId);
    }
    
    [Fact]
    public async Task ReturnSpansWithMatchingLinkFlagsProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Link = new LinkFilter
                {
                    Flags = new UInt32Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = spanToFind.Links[0].Flags
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Contains(response.Spans, flatSpan => flatSpan.Span.SpanId == spanToFind.SpanId);
    }
}