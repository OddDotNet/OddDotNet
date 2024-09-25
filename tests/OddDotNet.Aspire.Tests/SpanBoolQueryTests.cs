using OddDotNet.Proto.Spans.V1;

namespace OddDotNet.Aspire.Tests;

public class SpanBoolQueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanBoolQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(true, true, BoolCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(false, true, BoolCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    [InlineData(false, true, BoolCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
    [InlineData(true, true, BoolCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
    public async Task ReturnSpansWithMatchingBoolProperty(bool expected, bool actual,
        BoolCompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
        bool shouldBeIncluded)
    {
        // Arrange
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        var boolProperty = new BoolProperty
        {
            CompareAs = compareAs,
            Compare = expected
        };
        var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

        switch (propertyToCheck)
        {
            case WhereSpanPropertyFilter.PropertyOneofCase.Attribute:
                spanToFind.Attributes[0].Value.BoolValue = actual;
                spanToFind.Attributes[0].Key = "test";
                whereSpanPropertyFilter.Attribute = new KeyValueProperty() { Key = "test", BoolValue = boolProperty };
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