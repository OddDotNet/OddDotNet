using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Resource.V1;

namespace OddDotNet.Aspire.Tests.Metrics.V1;

public class MetricPropertyTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public MetricPropertyTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnMetricsWithMatchingNameProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty
                {
                    CompareAs = StringCompareAsType.Equals,
                    Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Name
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingDescriptionProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Description = new StringProperty
                {
                    CompareAs = StringCompareAsType.Equals,
                    Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Description
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingUnitProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Unit = new StringProperty
                {
                    CompareAs = StringCompareAsType.Equals,
                    Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Unit
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingMetadataProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Metadata = new KeyValueProperty
                {
                    Key = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Metadata[0].Key,
                    Value = new AnyValueProperty
                    {
                        StringValue = new StringProperty
                        {
                            CompareAs = StringCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Metadata[0].Value.StringValue
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
    public async Task ReturnMetricsWithMatchingInstrumentationScopeNameProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            InstrumentationScope = new InstrumentationScopeFilter
            {
                Name = new StringProperty
                {
                    CompareAs = StringCompareAsType.Equals,
                    Compare = request.ResourceMetrics[0].ScopeMetrics[0].Scope.Name
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingInstrumentationScopeAttributeProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            InstrumentationScope = new InstrumentationScopeFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = request.ResourceMetrics[0].ScopeMetrics[0].Scope.Attributes[0].Key,
                    Value = new AnyValueProperty
                    {
                        StringValue = new StringProperty
                        {
                            CompareAs = StringCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].ScopeMetrics[0].Scope.Attributes[0].Value.StringValue
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
    public async Task ReturnMetricsWithMatchingInstrumentationScopeVersionProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            InstrumentationScope = new InstrumentationScopeFilter
            {
                Version = new StringProperty
                {
                    CompareAs = StringCompareAsType.Equals,
                    Compare = request.ResourceMetrics[0].ScopeMetrics[0].Scope.Version
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingInstrumentationScopeDroppedAttributeCountProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            InstrumentationScope = new InstrumentationScopeFilter
            {
                DroppedAttributesCount = new UInt32Property
                {
                    CompareAs = NumberCompareAsType.Equals,
                    Compare = request.ResourceMetrics[0].ScopeMetrics[0].Scope.DroppedAttributesCount
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingInstrumentationScopeSchemaUrlProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            InstrumentationScopeSchemaUrl = new StringProperty
            {
                CompareAs = StringCompareAsType.Equals,
                Compare = request.ResourceMetrics[0].ScopeMetrics[0].SchemaUrl
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingResourceSchemaUrlProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ResourceSchemaUrl = new StringProperty
            {
                CompareAs = StringCompareAsType.Equals,
                Compare = request.ResourceMetrics[0].SchemaUrl
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
    
    [Fact]
    public async Task ReturnMetricsWithMatchingResourceAttributeProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Resource = new ResourceFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = request.ResourceMetrics[0].Resource.Attributes[0].Key,
                    Value = new AnyValueProperty
                    {
                        StringValue = new StringProperty
                        {
                            CompareAs = StringCompareAsType.Equals,
                            Compare = request.ResourceMetrics[0].Resource.Attributes[0].Value.StringValue
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
    public async Task ReturnMetricsWithMatchingResourceDroppedAttributeCountProperty()
    {
        var request = MetricHelpers.CreateExportMetricsServiceRequest();
        await _fixture.MetricsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Resource = new ResourceFilter
            {
                DroppedAttributesCount = new UInt32Property
                {
                    CompareAs = NumberCompareAsType.Equals,
                    Compare = request.ResourceMetrics[0].Resource.DroppedAttributesCount
                }
            }
        };
        
        var query = new MetricQueryRequest { Filters = { filter }};
        var response = await _fixture.MetricQueryServiceClient.QueryAsync(query);
        
        Assert.Single(response.Metrics);
    }
}