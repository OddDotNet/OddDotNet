using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Metrics.V1;

namespace OddDotNet.Aspire.Tests.Metrics.V1;

public class MetricExponentialHistogramTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public MetricExponentialHistogramTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointAttributeProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Attribute = new KeyValueProperty
                    {
                        Key = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Attributes[0].Key,
                        StringValue = new StringProperty
                        {
                            CompareAs = StringCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Attributes[0].Value.StringValue
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
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    StartTimeUnixNano = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].StartTimeUnixNano
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
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    TimeUnixNano = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].TimeUnixNano
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
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Count = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Count
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
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Sum = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Sum
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointScaleProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Scale = new Int32Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Scale
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointZeroCountProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    ZeroCount = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].ZeroCount
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointPositiveBucketOffsetProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Positive = new BucketFilter
                    {
                        Offset = new Int32Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Positive.Offset
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
    public async Task ReturnMetricsWithMatchingDataPointPositiveBucketCountProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Positive = new BucketFilter
                    {
                        BucketCount = new UInt64Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Positive.BucketCounts[0]
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
    public async Task ReturnMetricsWithMatchingDataPointNegativeBucketOffsetProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Negative = new BucketFilter
                    {
                        Offset = new Int32Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Negative.Offset
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
    public async Task ReturnMetricsWithMatchingDataPointExemplarTimeUnixNanoProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Exemplar = new ExemplarFilter
                    {
                        TimeUnixNano = new UInt64Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Exemplars[0].TimeUnixNano
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
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Flags = new UInt32Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Flags
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
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Min = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Min
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
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    Max = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].Max
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointZeroThresholdProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                DataPoint = new ExponentialHistogramDataPointFilter
                {
                    ZeroThreshold = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.DataPoints[0].ZeroThreshold
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
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0] = MetricHelpers.CreateMetric(MetricType.ExponentialHistogram);
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ExponentialHistogram = new ExponentialHistogramFilter
            {
                AggregationTemporality = new AggregationTemporalityProperty
                {
                    CompareAs = EnumCompareAsType.Equals,
                    Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].ExponentialHistogram.AggregationTemporality
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
}