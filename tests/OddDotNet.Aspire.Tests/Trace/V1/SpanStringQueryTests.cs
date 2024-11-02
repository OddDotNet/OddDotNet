using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Trace.V1;

// Don't put more tests in this file. It is using xunit class fixtures to spin up a single instance of the Aspire
// AppHost for performance. These tests need to run sequentially as they're using the same instance and need to perform
// cleanup between each run.
public class SpanStringQueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;
    
    public SpanStringQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Theory]
    [InlineData("test", "test", StringCompareAsType.Equals, PropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("other", "test", StringCompareAsType.Equals, PropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("other", "test", StringCompareAsType.NotEquals, PropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("test", "test", StringCompareAsType.NotEquals, PropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("te", "test", StringCompareAsType.Contains, PropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("other", "test", StringCompareAsType.Contains, PropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("other", "test", StringCompareAsType.NotContains, PropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("te", "test", StringCompareAsType.NotContains, PropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("", "", StringCompareAsType.IsEmpty, PropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("test", "test", StringCompareAsType.IsEmpty, PropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("test", "test", StringCompareAsType.IsNotEmpty, PropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("", "", StringCompareAsType.IsNotEmpty, PropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("test", "test", StringCompareAsType.Equals, PropertyFilter.ValueOneofCase.Attribute, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, PropertyFilter.ValueOneofCase.TraceState, true)]
    public async Task ReturnSpansWithMatchingStringProperty(string expected, string actual,
        StringCompareAsType compareAs, PropertyFilter.ValueOneofCase propertyToCheck,
        bool shouldBeIncluded)
    {
        // Arrange
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var resourceToFind = request.ResourceSpans[0];
        var scopeToFind = resourceToFind.ScopeSpans[0];
        var spanToFind = scopeToFind.Spans[0];
        
        
        var stringProperty = new StringProperty
        {
            CompareAs = compareAs,
            Compare = expected
        };
        var whereSpanPropertyFilter = new PropertyFilter();

        switch (propertyToCheck)
        {
            case PropertyFilter.ValueOneofCase.Name:
                spanToFind.Name = actual;
                whereSpanPropertyFilter.Name = stringProperty;
                break;
            case PropertyFilter.ValueOneofCase.TraceState:
                spanToFind.TraceState = actual;
                whereSpanPropertyFilter.TraceState = stringProperty;
                break;
            case PropertyFilter.ValueOneofCase.Attribute:
                spanToFind.Attributes[0].Value.StringValue = actual;
                spanToFind.Attributes[0].Key = "test";
                whereSpanPropertyFilter.Attribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
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

        var whereFilter = new Where()
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