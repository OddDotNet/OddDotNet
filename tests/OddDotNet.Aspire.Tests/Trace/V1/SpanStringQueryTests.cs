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
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("other", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("other", "test", StringCompareAsType.NotEquals, WherePropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("test", "test", StringCompareAsType.NotEquals, WherePropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("te", "test", StringCompareAsType.Contains, WherePropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("other", "test", StringCompareAsType.Contains, WherePropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("other", "test", StringCompareAsType.NotContains, WherePropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("te", "test", StringCompareAsType.NotContains, WherePropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("", "", StringCompareAsType.IsEmpty, WherePropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("test", "test", StringCompareAsType.IsEmpty, WherePropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("test", "test", StringCompareAsType.IsNotEmpty, WherePropertyFilter.ValueOneofCase.Name, true)]
    [InlineData("", "", StringCompareAsType.IsNotEmpty, WherePropertyFilter.ValueOneofCase.Name, false)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.EventName, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.LinkTraceState, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.Attribute, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.TraceState, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.EventAttribute, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.LinkAttribute, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.InstrumentationScopeName, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.InstrumentationScopeAttribute, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.InstrumentationScopeVersion, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.InstrumentationScopeSchemaUrl, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.ResourceSchemaUrl, true)]
    [InlineData("test", "test", StringCompareAsType.Equals, WherePropertyFilter.ValueOneofCase.ResourceAttribute, true)]
    public async Task ReturnSpansWithMatchingStringProperty(string expected, string actual,
        StringCompareAsType compareAs, WherePropertyFilter.ValueOneofCase propertyToCheck,
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
        var whereSpanPropertyFilter = new WherePropertyFilter();

        switch (propertyToCheck)
        {
            case WherePropertyFilter.ValueOneofCase.Name:
                spanToFind.Name = actual;
                whereSpanPropertyFilter.Name = stringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.EventName:
                spanToFind.Events[0].Name = actual;
                whereSpanPropertyFilter.EventName = stringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.TraceState:
                spanToFind.TraceState = actual;
                whereSpanPropertyFilter.TraceState = stringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.LinkTraceState:
                spanToFind.Links[0].TraceState = actual;
                whereSpanPropertyFilter.LinkTraceState = stringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.Attribute:
                spanToFind.Attributes[0].Value.StringValue = actual;
                spanToFind.Attributes[0].Key = "test";
                whereSpanPropertyFilter.Attribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
                break;
            case WherePropertyFilter.ValueOneofCase.EventAttribute:
                spanToFind.Events[0].Attributes[0].Value.StringValue = actual;
                spanToFind.Events[0].Attributes[0].Key = "test";
                whereSpanPropertyFilter.EventAttribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
                break;
            case WherePropertyFilter.ValueOneofCase.LinkAttribute:
                spanToFind.Links[0].Attributes[0].Value.StringValue = actual;
                spanToFind.Links[0].Attributes[0].Key = "test";
                whereSpanPropertyFilter.LinkAttribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
                break;
            case WherePropertyFilter.ValueOneofCase.InstrumentationScopeName:
                scopeToFind.Scope.Name = actual;
                whereSpanPropertyFilter.InstrumentationScopeName = stringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.InstrumentationScopeAttribute:
                scopeToFind.Scope.Attributes[0].Value.StringValue = actual;
                scopeToFind.Scope.Attributes[0].Key = "test";
                whereSpanPropertyFilter.InstrumentationScopeAttribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
                break;
            case WherePropertyFilter.ValueOneofCase.InstrumentationScopeVersion:
                scopeToFind.Scope.Version = actual;
                whereSpanPropertyFilter.InstrumentationScopeVersion = stringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.InstrumentationScopeSchemaUrl:
                scopeToFind.SchemaUrl = actual;
                whereSpanPropertyFilter.InstrumentationScopeSchemaUrl = stringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.ResourceSchemaUrl:
                resourceToFind.SchemaUrl = actual;
                whereSpanPropertyFilter.ResourceSchemaUrl = stringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.ResourceAttribute:
                resourceToFind.Resource.Attributes[0].Value.StringValue = actual;
                resourceToFind.Resource.Attributes[0].Key = "test";
                whereSpanPropertyFilter.ResourceAttribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
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