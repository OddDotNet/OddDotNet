using Google.Protobuf;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Trace.V1;

// Don't put more tests in this file. It is using xunit class fixtures to spin up a single instance of the Aspire
// AppHost for performance. These tests need to run sequentially as they're using the same instance and need to perform
// cleanup between each run.
public class SpanByteStringQueryTests : IClassFixture<AspireFixture>, IAsyncLifetime
{
    private readonly AspireFixture _fixture;

    public SpanByteStringQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.Equals,
        WherePropertyFilter.ValueOneofCase.SpanId,
        true)
    ]
    [InlineData(
        new byte[] { 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.Equals,
        WherePropertyFilter.ValueOneofCase.SpanId,
        false)
    ]
    [InlineData(
        new byte[] { 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.NotEquals,
        WherePropertyFilter.ValueOneofCase.SpanId,
        true)
    ]
    [InlineData(
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.NotEquals,
        WherePropertyFilter.ValueOneofCase.SpanId,
        false)
    ]
    [InlineData(
        new byte[] { },
        new byte[] { },
        ByteStringCompareAsType.Empty,
        WherePropertyFilter.ValueOneofCase.ParentSpanId,
        true)
    ]
    [InlineData(
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.Empty,
        WherePropertyFilter.ValueOneofCase.ParentSpanId,
        false)
    ]
    [InlineData(
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.NotEmpty,
        WherePropertyFilter.ValueOneofCase.ParentSpanId,
        true)
    ]
    [InlineData(
        new byte[] { },
        new byte[] { },
        ByteStringCompareAsType.NotEmpty,
        WherePropertyFilter.ValueOneofCase.ParentSpanId,
        false)
    ]
    [InlineData(
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.Equals,
        WherePropertyFilter.ValueOneofCase.TraceId,
        true)
    ]
    [InlineData(
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.Equals,
        WherePropertyFilter.ValueOneofCase.LinkTraceId,
        true)
    ]
    [InlineData(
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.Equals,
        WherePropertyFilter.ValueOneofCase.LinkSpanId,
        true)
    ]
    [InlineData(
        new byte[] { 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A },
        new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 },
        ByteStringCompareAsType.NotEquals,
        WherePropertyFilter.ValueOneofCase.Attribute,
        true)
    ]
    public async Task ReturnSpansWithMatchingByteStringProperty(byte[] expected, byte[] actual,
        ByteStringCompareAsType compareAs, WherePropertyFilter.ValueOneofCase propertyToCheck,
        bool shouldBeIncluded)
    {
        // Arrange
        var request = TraceHelpers.CreateExportTraceServiceRequest();
        var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
        var byteStringProperty = new ByteStringProperty
        {
            CompareAs = compareAs,
            Compare = ByteString.CopyFrom(expected)
        };
        var whereSpanPropertyFilter = new WherePropertyFilter();

        switch (propertyToCheck)
        {
            case WherePropertyFilter.ValueOneofCase.SpanId:
                spanToFind.SpanId = ByteString.CopyFrom(actual);
                whereSpanPropertyFilter.SpanId = byteStringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.TraceId:
                spanToFind.TraceId = ByteString.CopyFrom(actual);
                whereSpanPropertyFilter.TraceId = byteStringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.ParentSpanId:
                spanToFind.ParentSpanId = ByteString.CopyFrom(actual);
                whereSpanPropertyFilter.ParentSpanId = byteStringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.LinkTraceId:
                spanToFind.Links[0].TraceId = ByteString.CopyFrom(actual);
                whereSpanPropertyFilter.LinkTraceId = byteStringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.LinkSpanId:
                spanToFind.Links[0].SpanId = ByteString.CopyFrom(actual);
                whereSpanPropertyFilter.LinkSpanId = byteStringProperty;
                break;
            case WherePropertyFilter.ValueOneofCase.Attribute:
                spanToFind.Attributes[0].Value.BytesValue = ByteString.CopyFrom(actual);
                spanToFind.Attributes[0].Key = "test";
                whereSpanPropertyFilter.Attribute = new KeyValueProperty()
                    { ByteStringValue = byteStringProperty, Key = "test" };
                break;
        }

        // Send the trace
        await _fixture.TraceServiceClient.ExportAsync(request);

        //Act
        var take = new Take()
        {
            TakeFirst = new TakeFirst()
        };

        var duration = new Duration()
        {
            Milliseconds = 1000
        };

        var whereFilter = new WhereFilter()
        {
            Property = whereSpanPropertyFilter
        };

        var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter }, Duration = duration };

        var response = await _fixture.SpanQueryServiceClient.QueryAsync(spanQueryRequest);

        // Assert
        Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
        if (shouldBeIncluded)
            Assert.True(response.Spans[0].Span.SpanId == spanToFind.SpanId);
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _fixture.SpanQueryServiceClient.ResetAsync(new());
    }
}