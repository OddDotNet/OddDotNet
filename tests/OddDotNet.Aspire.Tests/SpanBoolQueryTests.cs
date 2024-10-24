using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests;

public class SpanBoolQueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanBoolQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(true, true, BoolCompareAsType.Equals, WhereSpanPropertyFilter.ValueOneofCase.Attribute, true)]
    [InlineData(false, true, BoolCompareAsType.Equals, WhereSpanPropertyFilter.ValueOneofCase.Attribute, false)]
    [InlineData(false, true, BoolCompareAsType.NotEquals, WhereSpanPropertyFilter.ValueOneofCase.Attribute, true)]
    [InlineData(true, true, BoolCompareAsType.NotEquals, WhereSpanPropertyFilter.ValueOneofCase.Attribute, false)]
    public async Task ReturnSpansWithMatchingBoolProperty(bool expected, bool actual,
        BoolCompareAsType compareAs, WhereSpanPropertyFilter.ValueOneofCase propertyToCheck,
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
            case WhereSpanPropertyFilter.ValueOneofCase.Attribute:
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