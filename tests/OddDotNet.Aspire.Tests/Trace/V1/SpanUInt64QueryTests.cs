using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Trace.V1;

public class SpanUInt64QueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanUInt64QueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(1L, 1L, NumberCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano, true)]
    [InlineData(0L, 1L, NumberCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano, false)]
    [InlineData(0L, 1L, NumberCompareAsType.NotEquals, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 1L, NumberCompareAsType.NotEquals, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(1L, 1L, NumberCompareAsType.GreaterThanEquals,
        WherePropertyFilter.ValueOneofCase.StartTimeUnixNano, true)]
    [InlineData(1L, 2L, NumberCompareAsType.GreaterThanEquals,
        WherePropertyFilter.ValueOneofCase.StartTimeUnixNano, true)]
    [InlineData(2L, 1L, NumberCompareAsType.GreaterThanEquals,
        WherePropertyFilter.ValueOneofCase.StartTimeUnixNano, false)]
    [InlineData(1L, 2L, NumberCompareAsType.GreaterThan, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 1L, NumberCompareAsType.GreaterThan, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(1L, 1L, NumberCompareAsType.LessThanEquals, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(2L, 1L, NumberCompareAsType.LessThanEquals, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 2L, NumberCompareAsType.LessThanEquals, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(2L, 1L, NumberCompareAsType.LessThan, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano,
        true)]
    [InlineData(1L, 1L, NumberCompareAsType.LessThan, WherePropertyFilter.ValueOneofCase.StartTimeUnixNano,
        false)]
    [InlineData(1L, 1L, NumberCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.EndTimeUnixNano, true)]
    [InlineData(1L, 1L, NumberCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.EventTimeUnixNano, true)]
    public async Task ReturnSpansWithMatchingUInt64Property(ulong expected, ulong actual,
        NumberCompareAsType compareAs, WherePropertyFilter.ValueOneofCase propertyToCheck,
        bool shouldBeIncluded)
    {
        // Arrange
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        var uInt64Property = new UInt64Property
        {
            CompareAs = compareAs,
            Compare = expected
        };
        var whereSpanPropertyFilter = new WherePropertyFilter();

        switch (propertyToCheck)
        {
            case WherePropertyFilter.ValueOneofCase.StartTimeUnixNano:
                spanToFind.StartTimeUnixNano = actual;
                whereSpanPropertyFilter.StartTimeUnixNano = uInt64Property;
                break;
            case WherePropertyFilter.ValueOneofCase.EndTimeUnixNano:
                spanToFind.EndTimeUnixNano = actual;
                whereSpanPropertyFilter.EndTimeUnixNano = uInt64Property;
                break;
            case WherePropertyFilter.ValueOneofCase.EventTimeUnixNano:
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

        var whereFilter = new WhereFilter()
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