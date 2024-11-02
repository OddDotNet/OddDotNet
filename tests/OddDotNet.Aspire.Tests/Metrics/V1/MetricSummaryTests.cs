using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Metrics.V1;

namespace OddDotNet.Aspire.Tests.Metrics.V1;

public class MetricSummaryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public MetricSummaryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointAttributeProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Summary);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Summary = new SummaryFilter
                {
                    DataPoint = new SummaryDataPointFilter
                    {
                        Attribute = new KeyValueProperty
                        {
                            Key = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Summary.DataPoints[0].Attributes[0].Key,
                            StringValue = new StringProperty
                            {
                                CompareAs = StringCompareAsType.Equals,
                                Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Summary.DataPoints[0].Attributes[0].Value.StringValue
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
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointStartTimeUnixNanoProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Summary);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Summary = new SummaryFilter
                {
                    DataPoint = new SummaryDataPointFilter
                    {
                        StartTimeUnixNano = new UInt64Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Summary.DataPoints[0].StartTimeUnixNano
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
    public async Task ReturnMetricsWithMatchingDataPointTimeUnixNanoProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Summary);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Summary = new SummaryFilter
                {
                    DataPoint = new SummaryDataPointFilter
                    {
                        TimeUnixNano = new UInt64Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Summary.DataPoints[0].TimeUnixNano
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
    public async Task ReturnMetricsWithMatchingDataPointCountProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Summary);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Summary = new SummaryFilter
                {
                    DataPoint = new SummaryDataPointFilter
                    {
                        Count = new UInt64Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Summary.DataPoints[0].Count
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
    public async Task ReturnMetricsWithMatchingDataPointSumProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Summary);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Summary = new SummaryFilter
                {
                    DataPoint = new SummaryDataPointFilter
                    {
                        Sum = new DoubleProperty
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Summary.DataPoints[0].Sum
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
    public async Task ReturnMetricsWithMatchingDataPointValueAtQuantileQuantileProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Summary);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Summary = new SummaryFilter
                {
                    DataPoint = new SummaryDataPointFilter
                    {
                        QuantileValue = new ValueAtQuantileFilter
                        {
                            Quantile = new DoubleProperty
                            {
                                CompareAs = NumberCompareAsType.Equals,
                                Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Summary.DataPoints[0].QuantileValues[0].Quantile
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
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointValueAtQuantileValueProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Summary);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Summary = new SummaryFilter
                {
                    DataPoint = new SummaryDataPointFilter
                    {
                        QuantileValue = new ValueAtQuantileFilter
                        {
                            Value = new DoubleProperty
                            {
                                CompareAs = NumberCompareAsType.Equals,
                                Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Summary.DataPoints[0].QuantileValues[0].Value
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
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointFlagsProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.Summary);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Summary = new SummaryFilter
                {
                    DataPoint = new SummaryDataPointFilter
                    {
                        Flags = new UInt32Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Summary.DataPoints[0].Flags
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