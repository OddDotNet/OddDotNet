using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests;

public class SpanUInt32QueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanUInt32QueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(1u, 1u, NumberCompareAsType.Equals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, true)]
    [InlineData(0u, 1u, NumberCompareAsType.Equals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, false)]
    [InlineData(0u, 1u, NumberCompareAsType.NotEquals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, true)]
    [InlineData(1u, 1u, NumberCompareAsType.NotEquals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, false)]
    [InlineData(1u, 1u, NumberCompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags,
        true)]
    [InlineData(1u, 2u, NumberCompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags,
        true)]
    [InlineData(2u, 1u, NumberCompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags,
        false)]
    [InlineData(1u, 2u, NumberCompareAsType.GreaterThan, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, true)]
    [InlineData(1u, 1u, NumberCompareAsType.GreaterThan, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, false)]
    [InlineData(1u, 1u, NumberCompareAsType.LessThanEquals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, true)]
    [InlineData(2u, 1u, NumberCompareAsType.LessThanEquals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, true)]
    [InlineData(1u, 2u, NumberCompareAsType.LessThanEquals, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, false)]
    [InlineData(2u, 1u, NumberCompareAsType.LessThan, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, true)]
    [InlineData(1u, 1u, NumberCompareAsType.LessThan, WhereSpanPropertyFilter.ValueOneofCase.LinkFlags, false)]
    [InlineData(1u, 1u, NumberCompareAsType.Equals, WhereSpanPropertyFilter.ValueOneofCase.Flags, true)]
    public async Task ReturnSpansWithMatchingUInt32Property(uint expected, uint actual,
        NumberCompareAsType compareAs, WhereSpanPropertyFilter.ValueOneofCase propertyToCheck,
        bool shouldBeIncluded)
    {
        // Arrange
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        var uInt32Property = new UInt32Property
        {
            CompareAs = compareAs,
            Compare = expected
        };
        var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

        switch (propertyToCheck)
        {
            case WhereSpanPropertyFilter.ValueOneofCase.LinkFlags:
                spanToFind.Links[0].Flags = actual;
                whereSpanPropertyFilter.LinkFlags = uInt32Property;
                break;
            case WhereSpanPropertyFilter.ValueOneofCase.Flags:
                spanToFind.Flags = actual;
                whereSpanPropertyFilter.Flags = uInt32Property;
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

        var whereFilter = new WhereSpanFilter()
        {
            SpanProperty = whereSpanPropertyFilter
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