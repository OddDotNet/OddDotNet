using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests;

public class SpanUInt32QueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanUInt32QueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    // [Theory]
    // [InlineData(1u, 1u, NumberCompareAsType.Equals, PropertyFilter.ValueOneofCase.LinkFlags, true)]
    // [InlineData(0u, 1u, NumberCompareAsType.Equals, PropertyFilter.ValueOneofCase.LinkFlags, false)]
    // [InlineData(0u, 1u, NumberCompareAsType.NotEquals, PropertyFilter.ValueOneofCase.LinkFlags, true)]
    // [InlineData(1u, 1u, NumberCompareAsType.NotEquals, PropertyFilter.ValueOneofCase.LinkFlags, false)]
    // [InlineData(1u, 1u, NumberCompareAsType.GreaterThanEquals, PropertyFilter.ValueOneofCase.LinkFlags,
    //     true)]
    // [InlineData(1u, 2u, NumberCompareAsType.GreaterThanEquals, PropertyFilter.ValueOneofCase.LinkFlags,
    //     true)]
    // [InlineData(2u, 1u, NumberCompareAsType.GreaterThanEquals, PropertyFilter.ValueOneofCase.LinkFlags,
    //     false)]
    // [InlineData(1u, 2u, NumberCompareAsType.GreaterThan, WherePropertyFilter.ValueOneofCase.LinkFlags, true)]
    // [InlineData(1u, 1u, NumberCompareAsType.GreaterThan, WherePropertyFilter.ValueOneofCase.LinkFlags, false)]
    // [InlineData(1u, 1u, NumberCompareAsType.LessThanEquals, WherePropertyFilter.ValueOneofCase.LinkFlags, true)]
    // [InlineData(2u, 1u, NumberCompareAsType.LessThanEquals, WherePropertyFilter.ValueOneofCase.LinkFlags, true)]
    // [InlineData(1u, 2u, NumberCompareAsType.LessThanEquals, WherePropertyFilter.ValueOneofCase.LinkFlags, false)]
    // [InlineData(2u, 1u, NumberCompareAsType.LessThan, WherePropertyFilter.ValueOneofCase.LinkFlags, true)]
    // [InlineData(1u, 1u, NumberCompareAsType.LessThan, WherePropertyFilter.ValueOneofCase.LinkFlags, false)]
    // [InlineData(1u, 1u, NumberCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.Flags, true)]
    // public async Task ReturnSpansWithMatchingUInt32Property(uint expected, uint actual,
    //     NumberCompareAsType compareAs, WherePropertyFilter.ValueOneofCase propertyToCheck,
    //     bool shouldBeIncluded)
    // {
    //     // Arrange
    //     var request = TraceHelpers.CreateExportTraceServiceRequest();
    //     var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
    //     var uInt32Property = new UInt32Property
    //     {
    //         CompareAs = compareAs,
    //         Compare = expected
    //     };
    //     var whereSpanPropertyFilter = new WherePropertyFilter();
    //
    //     switch (propertyToCheck)
    //     {
    //         case WherePropertyFilter.ValueOneofCase.LinkFlags:
    //             spanToFind.Links[0].Flags = actual;
    //             whereSpanPropertyFilter.LinkFlags = uInt32Property;
    //             break;
    //         case WherePropertyFilter.ValueOneofCase.Flags:
    //             spanToFind.Flags = actual;
    //             whereSpanPropertyFilter.Flags = uInt32Property;
    //             break;
    //     }
    //
    //     // Send the trace
    //     await _fixture.TraceServiceClient.ExportAsync(request);
    //
    //     //Act
    //     var take = new Take()
    //     {
    //         TakeFirst = new TakeFirst()
    //     };
    //
    //     var duration = new Duration()
    //     {
    //         Milliseconds = 1000
    //     };
    //
    //     var whereFilter = new WhereFilter()
    //     {
    //         Property = whereSpanPropertyFilter
    //     };
    //
    //     var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter }, Duration = duration };
    //
    //     var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);
    //
    //     // Assert
    //     Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
    //     if (shouldBeIncluded)
    //         Assert.True(response.Spans[0].Span.SpanId == spanToFind.SpanId);
    // }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _fixture.SpanQueryServiceClient.ResetAsync(new());
    }
}