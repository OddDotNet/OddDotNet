using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Proto.Collector.Trace.V1;
using System.Linq;

namespace OddDotNet.Aspire.Tests;

public class SpanQueryServiceTests : IAsyncLifetime
{
    #pragma warning disable CS8618
    private TraceService.TraceServiceClient _traceServiceClient;
    private SpanQueryService.SpanQueryServiceClient _spanQueryServiceClient;
    private DistributedApplication _app;
    #pragma warning disable CS8618

    public class WhereSpanPropertyShould : SpanQueryServiceTests
    {
        [Theory]
        [InlineData(1L, 1L, UInt64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
        [InlineData(0L, 1L, UInt64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, false)]
        [InlineData(0L, 1L, UInt64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
        [InlineData(1L, 1L, UInt64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, false)]
        [InlineData(1L, 1L, UInt64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
        [InlineData(1L, 2L, UInt64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
        [InlineData(2L, 1L, UInt64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, false)]
        [InlineData(1L, 2L, UInt64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
        [InlineData(1L, 1L, UInt64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, false)]
        [InlineData(1L, 1L, UInt64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
        [InlineData(2L, 1L, UInt64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
        [InlineData(1L, 2L, UInt64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, false)]
        [InlineData(2L, 1L, UInt64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, true)]
        [InlineData(1L, 1L, UInt64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano, false)]
        [InlineData(1L, 1L, UInt64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, true)]
        [InlineData(0L, 1L, UInt64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, false)]
        [InlineData(0L, 1L, UInt64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, true)]
        [InlineData(1L, 1L, UInt64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, false)]
        [InlineData(1L, 1L, UInt64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, true)]
        [InlineData(1L, 2L, UInt64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, true)]
        [InlineData(2L, 1L, UInt64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, false)]
        [InlineData(1L, 2L, UInt64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, true)]
        [InlineData(1L, 1L, UInt64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, false)]
        [InlineData(1L, 1L, UInt64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, true)]
        [InlineData(2L, 1L, UInt64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, true)]
        [InlineData(1L, 2L, UInt64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, false)]
        [InlineData(2L, 1L, UInt64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, true)]
        [InlineData(1L, 1L, UInt64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano, false)]
        public async Task ReturnSpansWithMatchingUInt64Property(ulong expected, ulong actual,
            UInt64CompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
            bool shouldBeIncluded)
        {
            // Arrange
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var uInt64Property = new UInt64Property
            {
                CompareAs = compareAs,
                Compare = expected
            };
            var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

            switch (propertyToCheck)
            {
                case WhereSpanPropertyFilter.PropertyOneofCase.StartTimeUnixNano:
                    spanToFind.StartTimeUnixNano = actual;
                    whereSpanPropertyFilter.StartTimeUnixNano = uInt64Property;
                    break;
            }
            
            // Send the trace
            await _traceServiceClient.ExportAsync(request);
            
            //Act
            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var duration = new Duration()
            {
                SecondsValue = 1
            };

            var whereFilter = new WhereSpanFilter()
            {
                SpanProperty = whereSpanPropertyFilter
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter }, Duration = duration };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);

            // Assert
            Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
            if (shouldBeIncluded)
                Assert.True(response.Spans[0].SpanId == spanToFind.SpanId);
        }
        
        [Fact]
        public async Task ReturnSpansWithMatchingStartTimeUnixNanoProperty()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new WhereSpanFilter()
            {
                SpanProperty = new WhereSpanPropertyFilter()
                {
                    StartTimeUnixNano = new UInt64Property()
                    {
                        Compare = spanToFind.StartTimeUnixNano,
                        CompareAs = UInt64CompareAsType.Equals
                    }
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter } };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }
        
        [Fact]
        public async Task ReturnSpansWithMatchingEndTimeUnixNanoProperty()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];

            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new WhereSpanFilter()
            {
                SpanProperty = new WhereSpanPropertyFilter()
                {
                    EndTimeUnixNano = new UInt64Property()
                    {
                        Compare = spanToFind.StartTimeUnixNano,
                        CompareAs = UInt64CompareAsType.Equals
                    }
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter } };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }
        
        [Fact]
        public async Task ReturnSpansWithMatchingStatusCodeProperty()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var codeToFind = (SpanStatusCode)spanToFind.Status.Code;

            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new WhereSpanFilter()
            {
                SpanProperty = new WhereSpanPropertyFilter()
                {
                    SpanStatusCode = new SpanStatusCodeProperty()
                    {
                        CompareAs = EnumCompareAsType.Equals,
                        Compare = codeToFind
                    }
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter } };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }
        
        [Fact]
        public async Task ReturnSpansWithMatchingKindProperty()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var codeToFind = (SpanKind)spanToFind.Kind;

            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new WhereSpanFilter()
            {
                SpanProperty = new WhereSpanPropertyFilter()
                {
                    SpanKind = new SpanKindProperty()
                    {
                        CompareAs = EnumCompareAsType.Equals,
                        Compare = codeToFind
                    }
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter } };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }

        [Fact]
        public async Task ReturnSpansWithMatchingSpanAttributeProperty()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new WhereSpanFilter()
            {
                SpanProperty = new WhereSpanPropertyFilter()
                {
                    SpanAttribute = new KeyValueProperty()
                    {
                        Key = spanToFind.Attributes[0].Key,
                        StringValue = new StringProperty()
                        {
                            Compare = spanToFind.Attributes[0].Value.StringValue,
                            CompareAs = StringCompareAsType.Equals
                        }
                    }
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter } };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }

        [Theory]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, true)]
        [InlineData("other", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, false)]
        [InlineData("other", "test", StringCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, true)]
        [InlineData("test", "test", StringCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, false)]
        [InlineData("te", "test", StringCompareAsType.Contains, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, true)]
        [InlineData("other", "test", StringCompareAsType.Contains, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, false)]
        [InlineData("other", "test", StringCompareAsType.NotContains, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, true)]
        [InlineData("te", "test", StringCompareAsType.NotContains, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, false)]
        [InlineData("", "", StringCompareAsType.IsEmpty, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, true)]
        [InlineData("test", "test", StringCompareAsType.IsEmpty, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, false)]
        [InlineData("test", "test", StringCompareAsType.IsNotEmpty, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, true)]
        [InlineData("", "", StringCompareAsType.IsNotEmpty, WhereSpanPropertyFilter.PropertyOneofCase.SpanName, false)]
        public async Task ReturnSpansWithMatchingStringFilter(string expected, string actual,
            StringCompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
            bool shouldBeIncluded)
        {
            // Arrange
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var stringProperty = new StringProperty
            {
                CompareAs = compareAs,
                Compare = expected
            };
            var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

            switch (propertyToCheck)
            {
                case WhereSpanPropertyFilter.PropertyOneofCase.SpanName:
                    spanToFind.Name = actual;
                    whereSpanPropertyFilter.SpanName = stringProperty;
                    break;
            }
            
            // Send the trace
            await _traceServiceClient.ExportAsync(request);
            
            //Act
            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var duration = new Duration()
            {
                SecondsValue = 1
            };

            var whereFilter = new WhereSpanFilter()
            {
                SpanProperty = whereSpanPropertyFilter
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter }, Duration = duration };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);

            // Assert
            Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
            if (shouldBeIncluded)
                Assert.True(response.Spans[0].SpanId == spanToFind.SpanId);
        }

        [Theory]
        [InlineData(new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.SpanId, true)]
        [InlineData(new byte[]{0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.SpanId, false)]
        [InlineData(new byte[]{0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.SpanId, true)]
        [InlineData(new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.SpanId, false)]
        [InlineData(new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, true)]
        [InlineData(new byte[]{0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, false)]
        [InlineData(new byte[]{0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, true)]
        [InlineData(new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, false)]
        [InlineData(new byte[]{}, new byte[]{}, ByteStringCompareAsType.Empty, WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, true)]
        [InlineData(new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.Empty, WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, false)]
        [InlineData(new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, ByteStringCompareAsType.NotEmpty, WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, true)]
        [InlineData(new byte[]{}, new byte[]{}, ByteStringCompareAsType.NotEmpty, WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, false)]
        [InlineData(
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.Equals, 
            WhereSpanPropertyFilter.PropertyOneofCase.TraceId, 
            true)
        ]
        [InlineData(
            new byte[]{0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.Equals, 
            WhereSpanPropertyFilter.PropertyOneofCase.TraceId, 
            false)
        ]
        [InlineData(
            new byte[]{0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.NotEquals, 
            WhereSpanPropertyFilter.PropertyOneofCase.TraceId, 
            true)
        ]
        [InlineData(
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.NotEquals, 
            WhereSpanPropertyFilter.PropertyOneofCase.TraceId, 
            false)
        ]
        public async Task ReturnSpansWithMatchingByteStringProperty(byte[] expected, byte[] actual,
            ByteStringCompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
            bool shouldBeIncluded)
        {
            // Arrange
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var byteStringProperty = new ByteStringProperty
            {
                CompareAs = compareAs,
                Compare = ByteString.CopyFrom(expected)
            };
            var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

            switch (propertyToCheck)
            {
                case WhereSpanPropertyFilter.PropertyOneofCase.SpanId:
                    spanToFind.SpanId = ByteString.CopyFrom(actual);
                    whereSpanPropertyFilter.SpanId = byteStringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.TraceId:
                    spanToFind.TraceId = ByteString.CopyFrom(actual);
                    whereSpanPropertyFilter.TraceId = byteStringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId:
                    spanToFind.ParentSpanId = ByteString.CopyFrom(actual);
                    whereSpanPropertyFilter.ParentSpanId = byteStringProperty;
                    break;
            }
            
            // Send the trace
            await _traceServiceClient.ExportAsync(request);
            
            //Act
            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var duration = new Duration()
            {
                SecondsValue = 1
            };

            var whereFilter = new WhereSpanFilter()
            {
                SpanProperty = whereSpanPropertyFilter
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter }, Duration = duration };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);

            // Assert
            Assert.Equal(shouldBeIncluded, response.Spans.Count > 0);
            if (shouldBeIncluded)
                Assert.True(response.Spans[0].SpanId == spanToFind.SpanId);
        }
    }

    public class TakeAllShould : SpanQueryServiceTests
    {
        // 3 traces are exported at 500, 1000, and 2000 ms. 
        [Theory]
        [InlineData(3, Duration.ValueOneofCase.SecondsValue, 3)] // Should return all traces
        [InlineData(1200, Duration.ValueOneofCase.MillisecondsValue, 2)] // Times out before 3rd trace is received
        [InlineData(1, Duration.ValueOneofCase.MinutesValue, 3)] // should return all traces
        public async Task ReturnAllSpansWithinTimeframe(uint takeDuration, Duration.ValueOneofCase takeDurationValue, int expectedCount)
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var duration = new Duration();
            switch (takeDurationValue)
            {
                case Duration.ValueOneofCase.SecondsValue:
                    duration.SecondsValue = takeDuration;
                    break;
                case Duration.ValueOneofCase.MillisecondsValue:
                    duration.MillisecondsValue = takeDuration;
                    break;
                case Duration.ValueOneofCase.MinutesValue:
                    duration.MinutesValue = takeDuration;
                    break;
            }
            
            var take = new Take
            {
                TakeAll = new TakeAll()
            };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, Duration = duration};
            
            // Start the query waiting for 3 seconds, and send spans at 500, 1000, 2000 ms
            var responseTask = _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            var exportFirst = ExportDelayedTrace(request, TimeSpan.FromMilliseconds(500));
            var exportSecond = ExportDelayedTrace(request, TimeSpan.FromMilliseconds(1000));
            var exportThird = ExportDelayedTrace(request, TimeSpan.FromMilliseconds(2000));

            await Task.WhenAll(responseTask.ResponseAsync, exportFirst, exportSecond, exportThird);

            var response = await responseTask;
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(expectedCount, response.Spans.Count); 
        }
    }

    private async Task ExportDelayedTrace(ExportTraceServiceRequest request, TimeSpan delay)
    {
        await Task.Delay(delay);
        await _traceServiceClient.ExportAsync(request);
    }

    /// <summary>
    /// Builds and starts the AppHost project, which has a single OddDotNet project defined within.
    /// Once started, configures the clients for exporting and querying.
    /// </summary>
    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();
        _app = await builder.BuildAsync();
            
        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();

        await resourceNotificationService.WaitForResourceAsync("odd").WaitAsync(TimeSpan.FromSeconds(30));

        var endpoint = _app.GetEndpoint("odd", "http");
        var traceServiceChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _traceServiceClient = new TraceService.TraceServiceClient(traceServiceChannel);
            
        var spanQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _spanQueryServiceClient = new SpanQueryService.SpanQueryServiceClient(spanQueryChannel);
    }

    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}