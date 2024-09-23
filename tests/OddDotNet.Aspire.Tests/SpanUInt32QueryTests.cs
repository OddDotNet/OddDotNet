namespace OddDotNet.Aspire.Tests;

public class SpanUInt32QueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanUInt32QueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(1u, 1u, UInt32CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
    [InlineData(0u, 1u, UInt32CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
    [InlineData(0u, 1u, UInt32CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
    [InlineData(1u, 1u, UInt32CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
    [InlineData(1u, 1u, UInt32CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags,
        true)]
    [InlineData(1u, 2u, UInt32CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags,
        true)]
    [InlineData(2u, 1u, UInt32CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags,
        false)]
    [InlineData(1u, 2u, UInt32CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
    [InlineData(1u, 1u, UInt32CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
    [InlineData(1u, 1u, UInt32CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
    [InlineData(2u, 1u, UInt32CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
    [InlineData(1u, 2u, UInt32CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
    [InlineData(2u, 1u, UInt32CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
    [InlineData(1u, 1u, UInt32CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
    [InlineData(1u, 1u, UInt32CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Flags, true)]
    public async Task ReturnSpansWithMatchingUInt32Property(uint expected, uint actual,
        UInt32CompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
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
            case WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags:
                spanToFind.Links[0].Flags = actual;
                whereSpanPropertyFilter.LinkFlags = uInt32Property;
                break;
            case WhereSpanPropertyFilter.PropertyOneofCase.Flags:
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
            SecondsValue = 1
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
            Assert.True(response.Spans[0].SpanId == spanToFind.SpanId);
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