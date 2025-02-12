using Google.Protobuf;
using Grpc.Core;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Logs.V1;
using OddDotNet.Proto.Resource.V1;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Aspire.Tests.Logs.V1;

public class LogPropertyTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    private readonly Duration _duration = new Duration
    {
        Milliseconds = 1000
    };

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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter, traceIdFilter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter, traceIdFilter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
                    IntValue = new Int64Property
                    {
                        CompareAs = NumberCompareAsType.Equals,
                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.IntValue
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingArrayBodyProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body = new AnyValue
        {
            ArrayValue = new ArrayValue
            {
                Values =
                {
                    new AnyValue
                    {
                        StringValue = "test"
                    }
                }
            }
        };
        await _fixture.LogsServiceClient.ExportAsync(request);
    
        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Body = new AnyValueProperty
                {
                    ArrayValue = new ArrayValueProperty
                    {
                        Values =
                        {
                            new AnyValueProperty
                            {
                                StringValue = new StringProperty
                                {
                                    CompareAs = StringCompareAsType.Equals,
                                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.ArrayValue.Values[0].StringValue
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingKvListBodyProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body = new AnyValue
        {
            KvlistValue = new KeyValueList
            {
                Values =
                {
                    new KeyValue
                    {
                        Key = "key",
                        Value = new AnyValue
                        {
                            StringValue = "test"
                        }
                    }
                }
            }
        };
        await _fixture.LogsServiceClient.ExportAsync(request);
    
        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Body = new AnyValueProperty
                {
                    KvlistValue = new KeyValueListProperty
                    {
                        Values =
                        {
                            new KeyValueProperty
                            {
                                Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.KvlistValue.Values[0].Key,
                                Value = new AnyValueProperty
                                {
                                    StringValue = new StringProperty
                                    {
                                        CompareAs = StringCompareAsType.Equals,
                                        Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.KvlistValue.Values[0].Value.StringValue
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithComplexBodyProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        
        // A Body, with a kvlist, which contains another kvlist, which contains
        // another kv that is an ArrayValue, which contains two entries, a string
        // and an int.
        request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body = new AnyValue
        {
            KvlistValue = new KeyValueList
            {
                Values =
                {
                    new KeyValue
                    {
                        Key = "key",
                        Value = new AnyValue
                        {
                            KvlistValue = new KeyValueList
                            {
                                Values =
                                {
                                    new KeyValue
                                    {
                                        Key = "key2",
                                        Value = new AnyValue
                                        {
                                            ArrayValue = new ArrayValue
                                            {
                                                Values =
                                                {
                                                    new AnyValue
                                                    {
                                                        StringValue = "test"
                                                    },
                                                    new AnyValue
                                                    {
                                                        IntValue = 123
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        await _fixture.LogsServiceClient.ExportAsync(request);
    
        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Body = new AnyValueProperty
                {
                    KvlistValue = new KeyValueListProperty
                    {
                        Values =
                        {
                            new KeyValueProperty
                            {
                                Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.KvlistValue.Values[0].Key,
                                Value = new AnyValueProperty
                                {
                                    KvlistValue = new KeyValueListProperty
                                    {
                                        Values =
                                        {
                                            new KeyValueProperty
                                            {
                                                Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.KvlistValue.Values[0].Value.KvlistValue.Values[0].Key,
                                                Value = new AnyValueProperty
                                                {
                                                    ArrayValue = new ArrayValueProperty
                                                    {
                                                        Values =
                                                        {
                                                            new AnyValueProperty
                                                            {
                                                                IntValue = new Int64Property
                                                                {
                                                                    CompareAs = NumberCompareAsType.Equals,
                                                                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Body.KvlistValue.Values[0].Value.KvlistValue.Values[0].Value.ArrayValue.Values[1].IntValue
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
                Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                            Value = new AnyValueProperty
                            {
                                StringValue = new StringProperty
                                {
                                    CompareAs = StringCompareAsType.Equals,
                                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.StringValue
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
                Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                            Value = new AnyValueProperty
                            {
                                ByteStringValue = new ByteStringProperty
                                {
                                    CompareAs = ByteStringCompareAsType.Equals,
                                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.BytesValue
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
                Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                            Value = new AnyValueProperty
                            {
                                IntValue = new Int64Property
                                {
                                    CompareAs = NumberCompareAsType.Equals,
                                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.IntValue
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
                Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                            Value = new AnyValueProperty
                            {
                                BoolValue = new BoolProperty
                                {
                                    CompareAs = BoolCompareAsType.Equals,
                                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.BoolValue
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
                Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                            Value = new AnyValueProperty
                            {
                                DoubleValue = new DoubleProperty
                                {
                                    CompareAs = NumberCompareAsType.Equals,
                                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.DoubleValue
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingArrayAttributeProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        var attributes = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes;
        attributes.Clear();
        attributes.Add(CommonHelpers.CreateKeyValue("key", new ArrayValue { Values = { new AnyValue { StringValue = "test" } }}));
        await _fixture.LogsServiceClient.ExportAsync(request);
    
        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                            Value = new AnyValueProperty
                            {
                                ArrayValue = new ArrayValueProperty
                                {
                                    Values =
                                    {
                                        new AnyValueProperty
                                        {
                                            StringValue = new StringProperty
                                            {
                                                CompareAs = StringCompareAsType.Equals,
                                                Compare = attributes[0].Value.ArrayValue.Values[0].StringValue
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsWithMatchingKvlistAttributeProperty()
    {
        var request = LogHelpers.CreateExportLogsServiceRequest();
        var attributes = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes;
        attributes.Clear();
        attributes.Add(CommonHelpers.CreateKeyValue("key", new KeyValueList { Values = { CommonHelpers.CreateKeyValue("key2", "test") }}));
        await _fixture.LogsServiceClient.ExportAsync(request);
    
        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Key,
                            Value = new AnyValueProperty
                            {
                                KvlistValue = new KeyValueListProperty
                                {
                                    Values =
                                    {
                                        new KeyValueProperty
                                        {
                                            Key = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.KvlistValue.Values[0].Key,
                                            Value = new AnyValueProperty
                                            {
                                                StringValue = new StringProperty
                                                {
                                                    CompareAs = StringCompareAsType.Equals,
                                                    Compare = request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].Attributes[0].Value.KvlistValue.Values[0].Value.StringValue
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
    
    [Fact]
    public async Task ReturnLogsAsStream()
    {
        var request1 = LogHelpers.CreateExportLogsServiceRequest();
        var request2 = LogHelpers.CreateExportLogsServiceRequest();
        await _fixture.LogsServiceClient.ExportAsync(request1);
        await _fixture.LogsServiceClient.ExportAsync(request2);

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
                            SpanId = new ByteStringProperty
                            {
                                CompareAs = ByteStringCompareAsType.Equals,
                                Compare = request1.ResourceLogs[0].ScopeLogs[0].LogRecords[0].SpanId
                            }
                        }
                    },
                    new Where
                    {
                        Property = new PropertyFilter
                        {
                            SpanId = new ByteStringProperty
                            {
                                CompareAs = ByteStringCompareAsType.Equals,
                                Compare = request2.ResourceLogs[0].ScopeLogs[0].LogRecords[0].SpanId
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration, Take = new Take{TakeAll = new TakeAll()}};
        List<FlatLog> logs = new List<FlatLog>();
        await foreach (FlatLog log in _fixture.LogQueryServiceClient.StreamQuery(query).ResponseStream.ReadAllAsync())
        {
            logs.Add(log);
        }
        
        Assert.Equal(2, logs.Count);
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
                Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = request.ResourceLogs[0].ScopeLogs[0].Scope.Attributes[0].Key,
                            Value = new AnyValueProperty
                            {
                                StringValue = new StringProperty
                                {
                                    CompareAs = StringCompareAsType.Equals,
                                    Compare = request.ResourceLogs[0].ScopeLogs[0].Scope.Attributes[0].Value.StringValue
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
                Attributes = new KeyValueListProperty
                {
                    Values =
                    {
                        new KeyValueProperty
                        {
                            Key = request.ResourceLogs[0].Resource.Attributes[0].Key,
                            Value = new AnyValueProperty
                            {
                                StringValue = new StringProperty
                                {
                                    CompareAs = StringCompareAsType.Equals,
                                    Compare = request.ResourceLogs[0].Resource.Attributes[0].Value.StringValue
                                }
                            }
                        }
                    }
                }
            }
        };
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
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
        
        var query = new LogQueryRequest { Filters = { filter }, Duration = _duration };
        var response = await _fixture.LogQueryServiceClient.QueryAsync(query);
        
        Assert.Contains(response.Logs, log => log.Log.TraceId == request.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId);
    }
}