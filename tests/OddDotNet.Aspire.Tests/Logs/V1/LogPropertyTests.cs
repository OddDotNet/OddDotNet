using Google.Protobuf;
using OddDotNet.Filters;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Logs.V1;
using OddDotNet.Proto.Resource.V1;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Aspire.Tests.Logs.V1;

public class LogPropertyTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public LogPropertyTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnLogsWithMatchingTimeUnixNanoProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                TimeUnixNano = new UInt64Property
                {
                    CompareAs = NumberCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TimeUnixNano
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingObservedTimeUnixNanoProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                ObservedTimeUnixNano = new UInt64Property
                {
                    CompareAs = NumberCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].ObservedTimeUnixNano
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingSeverityNumberProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                SeverityNumber = new SeverityNumberProperty
                {
                    CompareAs = EnumCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].SeverityNumber
                }
            }
        };
        
        // Need this extra filter because lots of tests may have the same severity level
        var traceIdFilter = new Where
        {
            Property = new PropertyFilter
            {
                TraceId = new ByteStringProperty
                {
                    CompareAs = ByteStringCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter, traceIdFilter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingSeverityTextProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                SeverityText = new StringProperty
                {
                    CompareAs = StringCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].SeverityText
                }
            }
        };
        
        // Need this extra filter because lots of tests may have the same severity level
        var traceIdFilter = new Where
        {
            Property = new PropertyFilter
            {
                TraceId = new ByteStringProperty
                {
                    CompareAs = ByteStringCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter, traceIdFilter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingStringBodyProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Body = new AnyValueProperty
                {
                    StringValue = new StringProperty
                    {
                        CompareAs = StringCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.StringValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingByteStringBodyProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body = new AnyValue
        {
            BytesValue = ByteString.CopyFrom([1, 2, 3])
        };
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Body = new AnyValueProperty
                {
                    ByteStringValue = new ByteStringProperty
                    {
                        CompareAs = ByteStringCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.BytesValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingInt64BodyProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body = new AnyValue
        {
            IntValue = 123L
        };
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Body = new AnyValueProperty
                {
                    Int64Value = new Int64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.IntValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingBoolBodyProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body = new AnyValue
        {
            BoolValue = true
        };
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Body = new AnyValueProperty
                {
                    BoolValue = new BoolProperty
                    {
                        CompareAs = BoolCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.BoolValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingDoubleBodyProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body = new AnyValue
        {
            DoubleValue = 123.0
        };
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Body = new AnyValueProperty
                {
                    DoubleValue = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.DoubleValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingStringAttributeProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                    StringValue = new StringProperty
                    {
                        CompareAs = StringCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.StringValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingByteStringAttributeProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes.Clear();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes.Add(CommonHelpers.CreateKeyValue("key", ByteString.CopyFrom([1, 2, 3])));
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                    ByteStringValue = new ByteStringProperty
                    {
                        CompareAs = ByteStringCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.BytesValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingInt64AttributeProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes.Clear();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes.Add(CommonHelpers.CreateKeyValue("key", 123));
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                    Int64Value = new Int64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.IntValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingBoolAttributeProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes.Clear();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes.Add(CommonHelpers.CreateKeyValue("key", true));
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                    BoolValue = new BoolProperty
                    {
                        CompareAs = BoolCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.BoolValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingDoubleAttributeProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes.Clear();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes.Add(CommonHelpers.CreateKeyValue("key", 123.0));
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                    DoubleValue = new DoubleProperty
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.DoubleValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingDroppedAttributesCountProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                DroppedAttributesCount = new UInt32Property
                {
                    CompareAs = NumberCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].DroppedAttributesCount
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingFlagsProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Flags = new UInt32Property
                {
                    CompareAs = NumberCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Flags
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingTraceIdProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                TraceId = new ByteStringProperty
                {
                    CompareAs = ByteStringCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingSpanIdProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                SpanId = new ByteStringProperty
                {
                    CompareAs = ByteStringCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].SpanId
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }

    [Fact]
    public async Task ReturnLogsWithMatchingOrFilter()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

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
                            Body = new AnyValueProperty
                            {
                                ByteStringValue = new ByteStringProperty
                                {
                                    CompareAs = ByteStringCompareAsType.Equals,
                                    Compare = ByteString.CopyFrom([1, 2, 3, 4, 5]) // Won't match
                                }
                            }
                        }
                    },
                    new Where
                    {
                        Property = new PropertyFilter
                        {
                            Body = new AnyValueProperty
                            {
                                StringValue = new StringProperty
                                {
                                    CompareAs = StringCompareAsType.Equals,
                                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.StringValue // Will match
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingInstrumentationScopeNameProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            InstrumentationScope = new InstrumentationScopeFilter
            {
                Name = new StringProperty
                {
                    CompareAs = StringCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].Scope.Name
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }};
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingInstrumentationScopeAttributeProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            InstrumentationScope = new InstrumentationScopeFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = request.ResourceLogs[0].ScopeLogs[0].Scope.Attributes[0].Key,
                    StringValue = new StringProperty
                    {
                        CompareAs = StringCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].Scope.Attributes[0].Value.StringValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }};
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingInstrumentationScopeVersionProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            InstrumentationScope = new InstrumentationScopeFilter
            {
                Version = new StringProperty
                {
                    CompareAs = StringCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].ScopeLogs[0].Scope.Version
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }};
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingResourceDroppedAttributesCountProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Resource = new ResourceFilter
            {
                DroppedAttributesCount = new UInt32Property
                {
                    CompareAs = NumberCompareAsType.Equals,
                    Compare = request.ResourceLogs[0].Resource.DroppedAttributesCount
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter } };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingResourceAttributeProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            Resource = new ResourceFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = request.ResourceLogs[0].Resource.Attributes[0].Key,
                    StringValue = new StringProperty
                    {
                        CompareAs = StringCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].Resource.Attributes[0].Value.StringValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }};
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingResourceSchemaUrlProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            ResourceSchemaUrl = new StringProperty
            {
                CompareAs = StringCompareAsType.Equals,
                Compare = request.ResourceLogs[0].SchemaUrl
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }};
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingInstrumentationScopeSchemaUrlProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request);

        var filter = new Where
        {
            InstrumentationScopeSchemaUrl = new StringProperty
            {
                CompareAs = StringCompareAsType.Equals,
                Compare = request.ResourceLogs[0].ScopeLogs[0].SchemaUrl
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }};
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
}