using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Trace.V1;

public class SpanOrFilterQueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanOrFilterQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnTrueWhenAnyFiltersTrue()
    {
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        
        var falseFirstPropertyFilter = new PropertyFilter()
        {
            Name = new StringProperty()
            {
                Compare = "Not a matching name",
                CompareAs = StringCompareAsType.Equals
            }
        };
        var falseFirstFilter = new Where()
        {
            Property = falseFirstPropertyFilter
        };
        
        var trueSecondPropertyFilter = new PropertyFilter()
        {
            Name = new StringProperty()
            {
                Compare = spanToFind.Name,
                CompareAs = StringCompareAsType.Equals
            }
        };

        var trueSecondFilter = new Where()
        {
            Property = trueSecondPropertyFilter
        };
        
        var whereSpanOrFilter = new OrFilter();
        whereSpanOrFilter.Filters.Add(falseFirstFilter);
        whereSpanOrFilter.Filters.Add(trueSecondFilter);

        var orFilter = new Where
        {
            Or = whereSpanOrFilter
        };
        
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
        
        var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { orFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        // Assert
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