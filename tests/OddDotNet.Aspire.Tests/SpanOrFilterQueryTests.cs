using OddDotNet.Proto.Spans.V1;

namespace OddDotNet.Aspire.Tests;

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
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        
        var falseFirstPropertyFilter = new WhereSpanPropertyFilter()
        {
            Name = new StringProperty()
            {
                Compare = "Not a matching name",
                CompareAs = StringCompareAsType.Equals
            }
        };
        var falseFirstFilter = new WhereSpanFilter()
        {
            SpanProperty = falseFirstPropertyFilter
        };
        
        var trueSecondPropertyFilter = new WhereSpanPropertyFilter()
        {
            Name = new StringProperty()
            {
                Compare = spanToFind.Name,
                CompareAs = StringCompareAsType.Equals
            }
        };

        var trueSecondFilter = new WhereSpanFilter()
        {
            SpanProperty = trueSecondPropertyFilter
        };
        
        var whereSpanOrFilter = new WhereSpanOrFilter();
        whereSpanOrFilter.Filters.Add(falseFirstFilter);
        whereSpanOrFilter.Filters.Add(trueSecondFilter);

        var orFilter = new WhereSpanFilter
        {
            SpanOr = whereSpanOrFilter
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
            SecondsValue = 1
        };
        
        var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { orFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        // Assert
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