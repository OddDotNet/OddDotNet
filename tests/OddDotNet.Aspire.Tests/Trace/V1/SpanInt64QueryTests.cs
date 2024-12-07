using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Trace.V1;

public class SpanInt64QueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanInt64QueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(1L, 1L, NumberCompareAsType.Equals, PropertyFilter.ValueOneofCase.Attributes, true)]
    [InlineData(0L, 1L, NumberCompareAsType.Equals, PropertyFilter.ValueOneofCase.Attributes, false)]
    [InlineData(0L, 1L, NumberCompareAsType.NotEquals, PropertyFilter.ValueOneofCase.Attributes, true)]
    [InlineData(1L, 1L, NumberCompareAsType.NotEquals, PropertyFilter.ValueOneofCase.Attributes, false)]
    [InlineData(1L, 1L, NumberCompareAsType.GreaterThanEquals, PropertyFilter.ValueOneofCase.Attributes,
        true)]
    [InlineData(1L, 2L, NumberCompareAsType.GreaterThanEquals, PropertyFilter.ValueOneofCase.Attributes,
        true)]
    [InlineData(2L, 1L, NumberCompareAsType.GreaterThanEquals, PropertyFilter.ValueOneofCase.Attributes,
        false)]
    [InlineData(1L, 2L, NumberCompareAsType.GreaterThan, PropertyFilter.ValueOneofCase.Attributes, true)]
    [InlineData(1L, 1L, NumberCompareAsType.GreaterThan, PropertyFilter.ValueOneofCase.Attributes, false)]
    [InlineData(1L, 1L, NumberCompareAsType.LessThanEquals, PropertyFilter.ValueOneofCase.Attributes, true)]
    [InlineData(2L, 1L, NumberCompareAsType.LessThanEquals, PropertyFilter.ValueOneofCase.Attributes, true)]
    [InlineData(1L, 2L, NumberCompareAsType.LessThanEquals, PropertyFilter.ValueOneofCase.Attributes, false)]
    [InlineData(2L, 1L, NumberCompareAsType.LessThan, PropertyFilter.ValueOneofCase.Attributes, true)]
    [InlineData(1L, 1L, NumberCompareAsType.LessThan, PropertyFilter.ValueOneofCase.Attributes, false)]
    public async Task ReturnSpansWithMatchingInt64Property(long expected, long actual,
        NumberCompareAsType compareAs, PropertyFilter.ValueOneofCase propertyToCheck,
        bool shouldBeIncluded)
    {
        // Arrange
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        var int64Property = new Int64Property
        {
            CompareAs = compareAs,
            Compare = expected
        };
        var whereSpanPropertyFilter = new PropertyFilter();

        switch (propertyToCheck)
        {
            case PropertyFilter.ValueOneofCase.Attributes:
                spanToFind.Attributes[0].Value.IntValue = actual;
                spanToFind.Attributes[0].Key = "test";
                whereSpanPropertyFilter.Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = "test",
                            Value = new AnyValueProperty
                            {
                                IntValue = int64Property
                            }
                        }
                    }
                };
                break;
        }

        // Send the trace
        await _fixture.TraceServiceClient.ExportAsync(request);

        //Act
        var take = new Take()
        {
            TakeFirst = new TakeFirst()
        };

        var duration = new Duration()
        {
            Milliseconds = 1000
        };

        var whereFilter = new Where()
        {
            Property = whereSpanPropertyFilter
        };

        var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        // Assert
        Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
        if (shouldBeIncluded)
            Assert.True(response.Spans[0].Span.SpanId == spanToFind.SpanId);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _fixture.SpanQueryServiceClient.ResetAsync(new());
    }
}