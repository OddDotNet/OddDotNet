using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Metrics.V1;

namespace OddDotNet.Aspire.Tests.Metrics.V1;

public class MetricTakeTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public MetricTakeTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TakeFirstReturnsFirstMatch()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var take = new Take
        {
            TakeFirst = new()
        };

        var queryRequest = new MetricQueryRequest { Take = take };
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(queryRequest);
        
        Assert.Single(response.Metrics);
    }

    [Fact]
    public async Task TakeExactReturnsExactMatch()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);
        await _fixture.MetricsServiceClient.ExportAsync(request); // Send it twice

        var take = new Take
        {
            TakeExact = new TakeExact
            {
                Count = 1 // take only 1
            }
        };

        var queryRequest = new MetricQueryRequest { Take = take };
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(queryRequest);
        
        Assert.Single(response.Metrics);
    }

    [Fact]
    public async Task TakeAllReturnsAllMatches()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var take = new Take
        {
            TakeAll = new()
        };

        // Duration required when calling TakeAll
        var duration = new Duration
        {
            Milliseconds = 1000
        };

        var queryRequest = new MetricQueryRequest { Take = take, Duration = duration };
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(queryRequest);
        
        Assert.NotEmpty(response.Metrics);
    }
}