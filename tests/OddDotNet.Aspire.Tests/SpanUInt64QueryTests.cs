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
    [InlineData(1L, 1L, NumberCompareAsType.Equals, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano, true)]
    [InlineData(0L, 1L, NumberCompareAsType.Equals, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano, false)]
    [InlineData(0L, 1L, NumberCompareAsType.NotEquals, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 1L, NumberCompareAsType.NotEquals, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(1L, 1L, NumberCompareAsType.GreaterThanEquals,
        WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano, true)]
    [InlineData(1L, 2L, NumberCompareAsType.GreaterThanEquals,
        WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano, true)]
    [InlineData(2L, 1L, NumberCompareAsType.GreaterThanEquals,
        WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano, false)]
    [InlineData(1L, 2L, NumberCompareAsType.GreaterThan, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 1L, NumberCompareAsType.GreaterThan, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(1L, 1L, NumberCompareAsType.LessThanEquals, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(2L, 1L, NumberCompareAsType.LessThanEquals, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 2L, NumberCompareAsType.LessThanEquals, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(2L, 1L, NumberCompareAsType.LessThan, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 1L, NumberCompareAsType.LessThan, WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(1L, 1L, NumberCompareAsType.Equals, WhereSpanPropertyFilter.ValueOneofCase.EndTimeUnixNano, true)]
    [InlineData(1L, 1L, NumberCompareAsType.Equals, WhereSpanPropertyFilter.ValueOneofCase.EventTimeUnixNano, true)]
    public async Task ReturnSpansWithMatchingUInt64Property(ulong expected, ulong actual,
        NumberCompareAsType compareAs, WhereSpanPropertyFilter.ValueOneofCase propertyToCheck,
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
            case WhereSpanPropertyFilter.ValueOneofCase.StartTimeUnixNano:
                spanToFind.StartTimeUnixNano = actual;
                whereSpanPropertyFilter.StartTimeUnixNano = uInt64Property;
                break;
            case WhereSpanPropertyFilter.ValueOneofCase.EndTimeUnixNano:
                spanToFind.EndTimeUnixNano = actual;
                whereSpanPropertyFilter.EndTimeUnixNano = uInt64Property;
                break;
            case WhereSpanPropertyFilter.ValueOneofCase.EventTimeUnixNano:
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