using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Trace.V1;

public class SpanEventTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public SpanEventTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnSpansWithMatchingTimeUnixNanoProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Event = new EventFilter
                {
                    TimeUnixNano = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = spanToFind.Events[0].TimeUnixNano
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Equal(spanToFind.SpanId, response.Spans[0].Span.SpanId);
    }
    
    [Fact]
    public async Task ReturnSpansWithMatchingNameProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Event = new EventFilter
                {
                    Name = new StringProperty
                    {
                        CompareAs = StringCompareAsType.Equals,
                        Compare = spanToFind.Events[0].Name
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Equal(spanToFind.SpanId, response.Spans[0].Span.SpanId);
    }
    
    [Fact]
    public async Task ReturnSpansWithMatchingAttributeProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Event = new EventFilter
                {
                    Attributes = new KeyValueListProperty
                    {
                        Values =
                        {
                            new KeyValueProperty
                            {
                                Key = spanToFind.Events[0].Attributes[0].Key,
                                Value = new AnyValueProperty
                                {
                                    StringValue = new StringProperty
                                    {
                                        CompareAs = StringCompareAsType.Equals,
                                        Compare = spanToFind.Events[0].Attributes[0].Value.StringValue
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Equal(spanToFind.SpanId, response.Spans[0].Span.SpanId);
    }
    
    [Fact]
    public async Task ReturnSpansWithMatchingDroppedAttributesCountProperty()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

        await _fixture.TraceServiceClient.ExportAsync(request);

        var query = new Where
        {
            Property = new PropertyFilter
            {
                Event = new EventFilter
                {
                    DroppedAttributesCount = new UInt32Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = spanToFind.Events[0].DroppedAttributesCount
                    }
                }
            }
        };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(new SpanQueryRequest { Filters = { query }});

        // Assert
        Assert.Equal(spanToFind.SpanId, response.Spans[0].Span.SpanId);
    }
}