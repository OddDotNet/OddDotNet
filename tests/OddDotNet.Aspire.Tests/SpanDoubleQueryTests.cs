namespace OddDotNet.Aspire.Tests;

public class SpanDoubleQueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    [Theory]
    [InlineData(1.0, 1.0, DoubleCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(0.0, 1.0, DoubleCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    [InlineData(0.0, 1.0, DoubleCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(1.0, 1.0, DoubleCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    [InlineData(1.0, 1.0, DoubleCompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute,
        true)]
    [InlineData(1.0, 2.0, DoubleCompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute,
        true)]
    [InlineData(2.0, 1.0, DoubleCompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute,
        false)]
    [InlineData(1.0, 2.0, DoubleCompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(1.0, 1.0, DoubleCompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    [InlineData(1.0, 1.0, DoubleCompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute,
        true)]
    [InlineData(2.0, 1.0, DoubleCompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute,
        true)]
    [InlineData(1.0, 2.0, DoubleCompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute,
        false)]
    [InlineData(2.0, 1.0, DoubleCompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(1.0, 1.0, DoubleCompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    public async Task ReturnSpansWithMatchingDoubleProperty(double expected, double actual,
        DoubleCompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
        bool shouldBeIncluded)
    {
        // Arrange
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        var doubleProperty = new DoubleProperty
        {
            CompareAs = compareAs,
            Compare = expected
        };
        var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

        switch (propertyToCheck)
        {
            case WhereSpanPropertyFilter.PropertyOneofCase.Attribute:
                spanToFind.Attributes[0].Value.DoubleValue = actual;
                spanToFind.Attributes[0].Key = "test";
                whereSpanPropertyFilter.Attribute = new KeyValueProperty()
                    { Key = "test", DoubleValue = doubleProperty };
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

    public SpanDoubleQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
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