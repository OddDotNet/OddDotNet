using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Metrics.V1;

namespace OddDotNet.Aspire.Tests.Metrics.V1;

public class MetricGaugeTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public MetricGaugeTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointFlagsProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    Flags = new UInt32Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Flags
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointAttributeProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    Attribute = new KeyValueProperty
                    {
                        Key = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Attributes[0].Key,
                        StringValue = new StringProperty
                        {
                            CompareAs = StringCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Attributes[0].Value.StringValue
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
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    StartTimeUnixNano = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].StartTimeUnixNano
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
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    TimeUnixNano = new UInt64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].TimeUnixNano
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointValueAsDoubleProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    ValueAsDouble = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].AsDouble
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointValueAsIntProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].AsInt = 123;
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    ValueAsInt = new Int64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].AsInt
                    }
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDataPointExemplarFilteredAttributeProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    Exemplar = new ExemplarFilter
                    {
                        FilteredAttribute = new KeyValueProperty
                        {
                            Key = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Exemplars[0].FilteredAttributes[0].Key,
                            StringValue = new StringProperty
                            {
                                CompareAs = StringCompareAsType.Equals,
                                Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Exemplars[0].FilteredAttributes[0].Value.StringValue
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
    public async Task ReturnMetricsWithMatchingDataPointExemplarTimeUnixNanoProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    Exemplar = new ExemplarFilter
                    {
                        TimeUnixNano = new UInt64Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Exemplars[0].TimeUnixNano
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
    public async Task ReturnMetricsWithMatchingDataPointExemplarValueAsDoubleProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    Exemplar = new ExemplarFilter
                    {
                        ValueAsDouble = new DoubleProperty
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Exemplars[0].AsDouble
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
    public async Task ReturnMetricsWithMatchingDataPointExemplarValueAsIntProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    Exemplar = new ExemplarFilter
                    {
                        ValueAsInt = new Int64Property
                        {
                            CompareAs = NumberCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Exemplars[0].AsInt
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
    public async Task ReturnMetricsWithMatchingDataPointExemplarSpanIdProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    Exemplar = new ExemplarFilter
                    {
                        SpanId = new ByteStringProperty
                        {
                            CompareAs = ByteStringCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Exemplars[0].SpanId
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
    public async Task ReturnMetricsWithMatchingDataPointExemplarTraceIdProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Gauge = new GaugeFilter
            {
                DataPoint = new NumberDataPointFilter
                {
                    Exemplar = new ExemplarFilter
                    {
                        TraceId = new ByteStringProperty
                        {
                            CompareAs = ByteStringCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Gauge.DataPoints[0].Exemplars[0].TraceId
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