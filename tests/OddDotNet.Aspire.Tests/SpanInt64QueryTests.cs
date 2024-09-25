using OddDotNet.Proto.Spans.V1;

namespace OddDotNet.Aspire.Tests;

public class SpanInt64QueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanInt64QueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(1L, 1L, Int64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(0L, 1L, Int64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    [InlineData(0L, 1L, Int64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(1L, 1L, Int64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    [InlineData(1L, 1L, Int64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute,
        true)]
    [InlineData(1L, 2L, Int64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute,
        true)]
    [InlineData(2L, 1L, Int64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute,
        false)]
    [InlineData(1L, 2L, Int64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(1L, 1L, Int64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    [InlineData(1L, 1L, Int64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(2L, 1L, Int64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(1L, 2L, Int64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    [InlineData(2L, 1L, Int64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(1L, 1L, Int64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    public async Task ReturnSpansWithMatchingInt64Property(long expected, long actual,
        Int64CompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
        bool shouldBeIncluded)
    {
        // Arrange
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        var int64Property = new Int64Property
        {
            CompareAs = compareAs,
            Compare = expected
        };
        var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

        switch (propertyToCheck)
        {
            case WhereSpanPropertyFilter.PropertyOneofCase.Attribute:
                spanToFind.Attributes[0].Value.IntValue = actual;
                spanToFind.Attributes[0].Key = "test";
                whereSpanPropertyFilter.Attribute = new KeyValueProperty() { Key = "test", Int64Value = int64Property };
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