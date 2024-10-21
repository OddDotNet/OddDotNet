using Google.Protobuf;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Aspire.Tests;

public class SpanArrayQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public SpanArrayQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("test", "test", ArrayCompareAsType.Contains, true)]
    [InlineData("test", "other", ArrayCompareAsType.Contains, false)]
    public async Task ReturnSpansWithMatchingArrayStringProperty(string expected, string actual,
        ArrayCompareAsType compareAs, bool shouldBeIncluded)
    {
        // Arrange
        const string key = "key";
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var resourceToFind = request.ResourceSpans[0];
        var scopeToFind = resourceToFind.ScopeSpans[0];
        var spanToFind = scopeToFind.Spans[0];
        ArrayValue arrayValue = new ArrayValue
        {
            Values = { new AnyValue { StringValue = actual } }
        };
        spanToFind.Attributes.Add(new KeyValue { Key = key, Value = new AnyValue { ArrayValue = arrayValue } });

        var spanIdFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                SpanId = new ByteStringProperty
                {
                    Compare = spanToFind.SpanId,
                    CompareAs = ByteStringCompareAsType.Equals
                }
            }
        };
        var arrayValueFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = key,
                    ArrayValue = new ArrayProperty
                    {
                        Compare = new AnyValue
                        {
                            StringValue = expected
                        },
                        CompareAs = compareAs
                    }
                }
            }
        };

        await _fixture.TraceServiceClient.ExportAsync(request);

        var take = new Take { TakeFirst = new() };
        var duration = new Duration { Milliseconds = 1000 };
        var spanQueryRequest = new SpanQueryRequest
            { Take = take, Filters = { spanIdFilter, arrayValueFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
    }

    [Theory]
    [InlineData(true, true, ArrayCompareAsType.Contains, true)]
    [InlineData(false, true, ArrayCompareAsType.Contains, false)]
    public async Task ReturnSpansWithMatchingArrayBoolProperty(bool expected, bool actual, ArrayCompareAsType compareAs,
        bool shouldBeIncluded)
    {
        // Arrange
        const string key = "key";
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var resourceToFind = request.ResourceSpans[0];
        var scopeToFind = resourceToFind.ScopeSpans[0];
        var spanToFind = scopeToFind.Spans[0];
        ArrayValue arrayValue = new ArrayValue
        {
            Values = { new AnyValue { BoolValue = actual } }
        };
        spanToFind.Attributes.Add(new KeyValue { Key = key, Value = new AnyValue { ArrayValue = arrayValue } });

        var spanIdFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                SpanId = new ByteStringProperty
                {
                    Compare = spanToFind.SpanId,
                    CompareAs = ByteStringCompareAsType.Equals
                }
            }
        };
        var arrayValueFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = key,
                    ArrayValue = new ArrayProperty
                    {
                        Compare = new AnyValue
                        {
                            BoolValue = expected
                        },
                        CompareAs = compareAs
                    }
                }
            }
        };

        await _fixture.TraceServiceClient.ExportAsync(request);

        var take = new Take { TakeFirst = new() };
        var duration = new Duration { Milliseconds = 1000 };
        var spanQueryRequest = new SpanQueryRequest
            { Take = take, Filters = { spanIdFilter, arrayValueFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
    }

    [Theory]
    [InlineData(1L, 1L, ArrayCompareAsType.Contains, true)]
    [InlineData(1L, 2L, ArrayCompareAsType.Contains, false)]
    public async Task ReturnSpansWithMatchingArrayInt64Property(long expected, long actual,
        ArrayCompareAsType compareAs, bool shouldBeIncluded)
    {
        // Arrange
        const string key = "key";
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var resourceToFind = request.ResourceSpans[0];
        var scopeToFind = resourceToFind.ScopeSpans[0];
        var spanToFind = scopeToFind.Spans[0];
        ArrayValue arrayValue = new ArrayValue
        {
            Values = { new AnyValue { IntValue = actual } }
        };
        spanToFind.Attributes.Add(new KeyValue { Key = key, Value = new AnyValue { ArrayValue = arrayValue } });

        var spanIdFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                SpanId = new ByteStringProperty
                {
                    Compare = spanToFind.SpanId,
                    CompareAs = ByteStringCompareAsType.Equals
                }
            }
        };
        var arrayValueFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = key,
                    ArrayValue = new ArrayProperty
                    {
                        Compare = new AnyValue
                        {
                            IntValue = expected
                        },
                        CompareAs = compareAs
                    }
                }
            }
        };

        await _fixture.TraceServiceClient.ExportAsync(request);

        var take = new Take { TakeFirst = new() };
        var duration = new Duration { Milliseconds = 1000 };
        var spanQueryRequest = new SpanQueryRequest
            { Take = take, Filters = { spanIdFilter, arrayValueFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
    }

    [Theory]
    [InlineData(1.0, 1.0, ArrayCompareAsType.Contains, true)]
    [InlineData(1.0, 2.0, ArrayCompareAsType.Contains, false)]
    public async Task ReturnSpansWithMatchingArrayDoubleProperty(double expected, double actual,
        ArrayCompareAsType compareAs, bool shouldBeIncluded)
    {
        // Arrange
        const string key = "key";
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var resourceToFind = request.ResourceSpans[0];
        var scopeToFind = resourceToFind.ScopeSpans[0];
        var spanToFind = scopeToFind.Spans[0];
        ArrayValue arrayValue = new ArrayValue
        {
            Values = { new AnyValue { DoubleValue = actual } }
        };
        spanToFind.Attributes.Add(new KeyValue { Key = key, Value = new AnyValue { ArrayValue = arrayValue } });

        var spanIdFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                SpanId = new ByteStringProperty
                {
                    Compare = spanToFind.SpanId,
                    CompareAs = ByteStringCompareAsType.Equals
                }
            }
        };
        var arrayValueFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = key,
                    ArrayValue = new ArrayProperty
                    {
                        Compare = new AnyValue
                        {
                            DoubleValue = expected
                        },
                        CompareAs = compareAs
                    }
                }
            }
        };

        await _fixture.TraceServiceClient.ExportAsync(request);

        var take = new Take { TakeFirst = new() };
        var duration = new Duration { Milliseconds = 1000 };
        var spanQueryRequest = new SpanQueryRequest
            { Take = take, Filters = { spanIdFilter, arrayValueFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
    }

    [Theory]
    [InlineData(0x41, 0x41, ArrayCompareAsType.Contains, true)]
    [InlineData(0x41, 0x42, ArrayCompareAsType.Contains, false)]
    public async Task ReturnSpansWithMatchingArrayBytesProperty(byte expected, byte actual,
        ArrayCompareAsType compareAs, bool shouldBeIncluded)
    {
        // Arrange
        const string key = "key";
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var resourceToFind = request.ResourceSpans[0];
        var scopeToFind = resourceToFind.ScopeSpans[0];
        var spanToFind = scopeToFind.Spans[0];
        ArrayValue arrayValue = new ArrayValue
        {
            Values = { new AnyValue { BytesValue = ByteString.CopyFrom(actual) } }
        };
        spanToFind.Attributes.Add(new KeyValue { Key = key, Value = new AnyValue { ArrayValue = arrayValue } });

        var spanIdFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                SpanId = new ByteStringProperty
                {
                    Compare = spanToFind.SpanId,
                    CompareAs = ByteStringCompareAsType.Equals
                }
            }
        };
        var arrayValueFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = key,
                    ArrayValue = new ArrayProperty
                    {
                        Compare = new AnyValue
                        {
                            BytesValue = ByteString.CopyFrom(expected)
                        },
                        CompareAs = compareAs
                    }
                }
            }
        };

        await _fixture.TraceServiceClient.ExportAsync(request);

        var take = new Take { TakeFirst = new() };
        var duration = new Duration { Milliseconds = 1000 };
        var spanQueryRequest = new SpanQueryRequest
            { Take = take, Filters = { spanIdFilter, arrayValueFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
    }

    [Theory]
    [InlineData(new object[] { "test", 123L }, new object[] { "test", 123L, 0x41 }, ArrayCompareAsType.Contains, true)]
    [InlineData(new object[] { "test", 123L }, new object[] { "test", 456L, 0x41 }, ArrayCompareAsType.Contains, false)]
    [InlineData(new object[] { "test", 123L }, new object[] { "test", 456L, 0x41 }, ArrayCompareAsType.DoesNotContain, true)]
    [InlineData(new object[] { "test", 123L }, new object[] { "test", 123L, 0x41 }, ArrayCompareAsType.DoesNotContain, false)]
    public async Task ReturnSpansWithMatchingArrayArrayValueProperty(object[] expected, object[] actual,
        ArrayCompareAsType compareAs, bool shouldBeIncluded)
    {
        // Arrange
        const string key = "key";
        var request = TestHelpers.CreateExportTraceServiceRequest();
        var resourceToFind = request.ResourceSpans[0];
        var scopeToFind = resourceToFind.ScopeSpans[0];
        var spanToFind = scopeToFind.Spans[0];
        byte actualByte = Convert.ToByte(actual[2]);
        AnyValue[] actualValues =
        [
            new AnyValue
            {
                StringValue = (string)actual[0]
            },
            new AnyValue
            {
                IntValue = (long)actual[1]
            },
            new AnyValue
            {
                BytesValue = ByteString.CopyFrom(actualByte)
            }
        ];
        AnyValue[] expectedValues =
        [
            new AnyValue
            {
                StringValue = (string)expected[0]
            },
            new AnyValue
            {
                IntValue = (long)expected[1]
            }
        ];
        ArrayValue actualArrayValue = new ArrayValue
        {
            Values = { actualValues }
        };
        ArrayValue expectedArrayValue = new ArrayValue
        {
            Values = { expectedValues }
        };

        spanToFind.Attributes.Add(new KeyValue { Key = key, Value = new AnyValue { ArrayValue = actualArrayValue } });

        var spanIdFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                SpanId = new ByteStringProperty
                {
                    Compare = spanToFind.SpanId,
                    CompareAs = ByteStringCompareAsType.Equals
                }
            }
        };
        var arrayValueFilter = new WhereSpanFilter
        {
            SpanProperty = new WhereSpanPropertyFilter
            {
                Attribute = new KeyValueProperty
                {
                    Key = key,
                    ArrayValue = new ArrayProperty
                    {
                        Compare = new AnyValue
                        {
                            ArrayValue = expectedArrayValue
                        },
                        CompareAs = compareAs
                    }
                }
            }
        };

        await _fixture.TraceServiceClient.ExportAsync(request);

        var take = new Take { TakeFirst = new() };
        var duration = new Duration { Milliseconds = 1000 };
        var spanQueryRequest = new SpanQueryRequest
            { Take = take, Filters = { spanIdFilter, arrayValueFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
    }
}