using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Metrics.V1;

namespace OddDotNet.Aspire.Tests.Metrics.V1;

public class MetricHistogramTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public MetricHistogramTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointAttributeProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    Attribute = new KeyValueProperty
                    {
                        Key = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].Attributes[0].Key,
                        StringValue = new StringProperty
                        {
                            CompareAs = StringCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].Attributes[0].Value.StringValue
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
    public async Task ReturnMetricsWithMatchingDataPointStartTimeUnixNanoProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    StartTimeUnixNano = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].StartTimeUnixNano
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointTimeUnixNanoProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    TimeUnixNano = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].TimeUnixNano
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointCountProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    Count = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].Count
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointSumProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    Sum = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].Sum
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointBucketCountProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    BucketCount = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].BucketCounts[0]
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointExplicitBoundProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    ExplicitBound = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].ExplicitBounds[0]
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointExemplarTimeUnixNanoProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    Exemplar = new ExemplarFilter
                    {
                        TimeUnixNano = new UInt64Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].Exemplars[0].TimeUnixNano
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
    public async Task ReturnMetricsWithMatchingDataPointFlagsProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    Flags = new UInt32Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].Flags
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointMinProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    Min = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].Min
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointMaxProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                DataPoint = new HistogramDataPointFilter
                {
                    Max = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.DataPoints[0].Max
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingAggregationTemporalityProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Histogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Histogram = new HistogramFilter
            {
                AggregationTemporality = new AggregationTemporalityProperty
                {
                    CompareAs = EnumCompareAsType.Equals,
                    Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Histogram.AggregationTemporality
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
}