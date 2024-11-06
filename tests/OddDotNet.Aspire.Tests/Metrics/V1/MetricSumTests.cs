using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Metrics.V1;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Aspire.Tests.Metrics.V1;

public class MetricSumTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public MetricSumTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnMetricsWithMatchingDataPoint()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Sum);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Sum = new SumFilter
                {
                    DataPoint = new NumberDataPointFilter
                    {
                        Flags = new UInt32Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Sum.DataPoints[0].Flags
                        }
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingAggregationTemporality()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Sum);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Sum = new SumFilter
                {
                    AggregationTemporality = new AggregationTemporalityProperty
                    {
                        CompareAs = EnumCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Sum.AggregationTemporality
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.NotEmpty(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingIsMonotonic()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Sum);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Sum = new SumFilter
                {
                    IsMonotonic = new BoolProperty
                    {
                        CompareAs = BoolCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Sum.IsMonotonic
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.NotEmpty(response.Metrics);
    }
}