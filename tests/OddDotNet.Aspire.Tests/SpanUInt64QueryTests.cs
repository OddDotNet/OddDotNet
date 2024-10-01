using OddDotNet.Proto.Spans.V1;

namespace OddDotNet.Aspire.Tests;

public class SpanUInt64QueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanUInt64QueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(1L, 1L, UInt64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
    [InlineData(0L, 1L, UInt64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, false)]
    [InlineData(0L, 1L, UInt64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 1L, UInt64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(1L, 1L, UInt64CompareAsType.GreaterThanEquals,
        WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
    [InlineData(1L, 2L, UInt64CompareAsType.GreaterThanEquals,
        WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
    [InlineData(2L, 1L, UInt64CompareAsType.GreaterThanEquals,
        WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, false)]
    [InlineData(1L, 2L, UInt64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 1L, UInt64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(1L, 1L, UInt64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(2L, 1L, UInt64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 2L, UInt64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(2L, 1L, UInt64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 1L, UInt64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(1L, 1L, UInt64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, true)]
    [InlineData(1L, 1L, UInt64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.EventTimeUnixNano, true)]
    public async Task ReturnSpansWithMatchingUInt64Property(ulong expected, ulong actual,
        UInt64CompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
        bool shouldBeIncluded)
    {
        // Arrange
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        var uInt64Property = new UInt64Property
        {
            CompareAs = compareAs,
            Compare = expected
        };
        var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

        switch (propertyToCheck)
        {
            case WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano:
                spanToFind.StartTimeUnixNano = actual;
                whereSpanPropertyFilter.StartTimeUnixNano = uInt64Property;
                break;
            case WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano:
                spanToFind.EndTimeUnixNano = actual;
                whereSpanPropertyFilter.EndTimeUnixNano = uInt64Property;
                break;
            case WhereSpanPropertyFilter.PropertyOneofCase.EventTimeUnixNano:
                spanToFind.Events[0].TimeUnixNano = actual;
                whereSpanPropertyFilter.EventTimeUnixNano = uInt64Property;
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