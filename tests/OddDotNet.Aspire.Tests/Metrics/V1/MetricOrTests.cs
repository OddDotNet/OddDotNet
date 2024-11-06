using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Metrics.V1;

namespace OddDotNet.Aspire.Tests.Metrics.V1;

public class MetricOrTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public MetricOrTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task ReturnMetricsWithOneTrueFilter()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        // Mutually exclusive Or filters
        var filter = new Where
        {
            Or = new OrFilter
            {
                Filters =
                {
                    new Where
                    {
                        Property = new PropertyFilter
                        {
                            Name = new StringProperty
                            {
                                CompareAs = StringCompareAsType.IsEmpty
                            }
                        }
                    },
                    new Where
                    {
                        Property = new PropertyFilter
                        {
                            Name = new StringProperty
                            {
                                CompareAs = StringCompareAsType.IsNotEmpty
                            }
                        }
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
}