using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Proto.Collector.Trace.V1;

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
        [InlineData(1L, 1L, UInt64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.EventTimeUnixNano, true)]
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
                case WhereSpanPropertyFilter.PropertyOneofCase.EndTimeUnixNano:
                    spanToFind.EndTimeUnixNano = actual;
                    whereSpanPropertyFilter.EndTimeUnixNano = uInt64Property;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.EventTimeUnixNano:
                    spanToFind.Events[0].TimeUnixNano = actual;
                    whereSpanPropertyFilter.EventTimeUnixNano = uInt64Property;
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
        [InlineData(1u, 1u, UInt32CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
        [InlineData(0u, 1u, UInt32CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
        [InlineData(0u, 1u, UInt32CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
        [InlineData(1u, 1u, UInt32CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
        [InlineData(1u, 1u, UInt32CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
        [InlineData(1u, 2u, UInt32CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
        [InlineData(2u, 1u, UInt32CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
        [InlineData(1u, 2u, UInt32CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
        [InlineData(1u, 1u, UInt32CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
        [InlineData(1u, 1u, UInt32CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
        [InlineData(2u, 1u, UInt32CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
        [InlineData(1u, 2u, UInt32CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
        [InlineData(2u, 1u, UInt32CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, true)]
        [InlineData(1u, 1u, UInt32CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags, false)]
        [InlineData(1u, 1u, UInt32CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Flags, true)]
        public async Task ReturnSpansWithMatchingUInt32Property(uint expected, uint actual,
            UInt32CompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
            bool shouldBeIncluded)
        {
            // Arrange
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var uInt32Property = new UInt32Property
            {
                CompareAs = compareAs,
                Compare = expected
            };
            var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

            switch (propertyToCheck)
            {
                case WhereSpanPropertyFilter.PropertyOneofCase.LinkFlags:
                    spanToFind.Links[0].Flags = actual;
                    whereSpanPropertyFilter.LinkFlags = uInt32Property;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.Flags:
                    spanToFind.Flags = actual;
                    whereSpanPropertyFilter.Flags = uInt32Property;
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
        [InlineData(1L, 1L, Int64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(0L, 1L, Int64CompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(0L, 1L, Int64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1L, 1L, Int64CompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(1L, 1L, Int64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1L, 2L, Int64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(2L, 1L, Int64CompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(1L, 2L, Int64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1L, 1L, Int64CompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(1L, 1L, Int64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(2L, 1L, Int64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1L, 2L, Int64CompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(2L, 1L, Int64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1L, 1L, Int64CompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        public async Task ReturnSpansWithMatchingInt64Property(long expected, long actual,
            Int64CompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
            bool shouldBeIncluded)
        {
            // Arrange
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var int64Property = new Int64Property
            {
                CompareAs = compareAs,
                Compare = expected
            };
            var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

            switch (propertyToCheck)
            {
                case WhereSpanPropertyFilter.PropertyOneofCase.Attribute:
                    spanToFind.Attributes[0].Value.IntValue = actual;
                    spanToFind.Attributes[0].Key = "test";
                    whereSpanPropertyFilter.Attribute = new KeyValueProperty(){ Key = "test", Int64Value = int64Property};
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
        [InlineData(1.0, 1.0, DoubleCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(0.0, 1.0, DoubleCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(0.0, 1.0, DoubleCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1.0, 1.0, DoubleCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(1.0, 1.0, DoubleCompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1.0, 2.0, DoubleCompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(2.0, 1.0, DoubleCompareAsType.GreaterThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(1.0, 2.0, DoubleCompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1.0, 1.0, DoubleCompareAsType.GreaterThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(1.0, 1.0, DoubleCompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(2.0, 1.0, DoubleCompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1.0, 2.0, DoubleCompareAsType.LessThanEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(2.0, 1.0, DoubleCompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(1.0, 1.0, DoubleCompareAsType.LessThan, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        public async Task ReturnSpansWithMatchingDoubleProperty(double expected, double actual,
            DoubleCompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
            bool shouldBeIncluded)
        {
            // Arrange
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var doubleProperty = new DoubleProperty
            {
                CompareAs = compareAs,
                Compare = expected
            };
            var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

            switch (propertyToCheck)
            {
                case WhereSpanPropertyFilter.PropertyOneofCase.Attribute:
                    spanToFind.Attributes[0].Value.DoubleValue = actual;
                    spanToFind.Attributes[0].Key = "test";
                    whereSpanPropertyFilter.Attribute = new KeyValueProperty(){ Key = "test", DoubleValue = doubleProperty};
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
        [InlineData(true, true, BoolCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(false, true, BoolCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        [InlineData(false, true, BoolCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData(true, true, BoolCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, false)]
        public async Task ReturnSpansWithMatchingBoolProperty(bool expected, bool actual,
            BoolCompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
            bool shouldBeIncluded)
        {
            // Arrange
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var boolProperty = new BoolProperty
            {
                CompareAs = compareAs,
                Compare = expected
            };
            var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

            switch (propertyToCheck)
            {
                case WhereSpanPropertyFilter.PropertyOneofCase.Attribute:
                    spanToFind.Attributes[0].Value.BoolValue = actual;
                    spanToFind.Attributes[0].Key = "test";
                    whereSpanPropertyFilter.Attribute = new KeyValueProperty(){ Key = "test", BoolValue = boolProperty};
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
                    StatusCode = new SpanStatusCodeProperty()
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
                    Kind = new SpanKindProperty()
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

        [Theory]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Name, true)]
        [InlineData("other", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Name, false)]
        [InlineData("other", "test", StringCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Name, true)]
        [InlineData("test", "test", StringCompareAsType.NotEquals, WhereSpanPropertyFilter.PropertyOneofCase.Name, false)]
        [InlineData("te", "test", StringCompareAsType.Contains, WhereSpanPropertyFilter.PropertyOneofCase.Name, true)]
        [InlineData("other", "test", StringCompareAsType.Contains, WhereSpanPropertyFilter.PropertyOneofCase.Name, false)]
        [InlineData("other", "test", StringCompareAsType.NotContains, WhereSpanPropertyFilter.PropertyOneofCase.Name, true)]
        [InlineData("te", "test", StringCompareAsType.NotContains, WhereSpanPropertyFilter.PropertyOneofCase.Name, false)]
        [InlineData("", "", StringCompareAsType.IsEmpty, WhereSpanPropertyFilter.PropertyOneofCase.Name, true)]
        [InlineData("test", "test", StringCompareAsType.IsEmpty, WhereSpanPropertyFilter.PropertyOneofCase.Name, false)]
        [InlineData("test", "test", StringCompareAsType.IsNotEmpty, WhereSpanPropertyFilter.PropertyOneofCase.Name, true)]
        [InlineData("", "", StringCompareAsType.IsNotEmpty, WhereSpanPropertyFilter.PropertyOneofCase.Name, false)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.EventName, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.LinkTraceState, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.Attribute, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.TraceState, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.EventAttribute, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.LinkAttribute, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.InstrumentationScopeName, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.InstrumentationScopeAttribute, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.InstrumentationScopeVersion, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.InstrumentationScopeSchemaUrl, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.ResourceSchemaUrl, true)]
        [InlineData("test", "test", StringCompareAsType.Equals, WhereSpanPropertyFilter.PropertyOneofCase.ResourceAttribute, true)]
        public async Task ReturnSpansWithMatchingStringProperty(string expected, string actual,
            StringCompareAsType compareAs, WhereSpanPropertyFilter.PropertyOneofCase propertyToCheck,
            bool shouldBeIncluded)
        {
            // Arrange
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var resourceToFind = request.ResourceSpans[0];
            var scopeToFind = resourceToFind.ScopeSpans[0];
            var spanToFind = scopeToFind.Spans[0];
            
            
            var stringProperty = new StringProperty
            {
                CompareAs = compareAs,
                Compare = expected
            };
            var whereSpanPropertyFilter = new WhereSpanPropertyFilter();

            switch (propertyToCheck)
            {
                case WhereSpanPropertyFilter.PropertyOneofCase.Name:
                    spanToFind.Name = actual;
                    whereSpanPropertyFilter.Name = stringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.EventName:
                    spanToFind.Events[0].Name = actual;
                    whereSpanPropertyFilter.EventName = stringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.TraceState:
                    spanToFind.TraceState = actual;
                    whereSpanPropertyFilter.TraceState = stringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.LinkTraceState:
                    spanToFind.Links[0].TraceState = actual;
                    whereSpanPropertyFilter.LinkTraceState = stringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.Attribute:
                    spanToFind.Attributes[0].Value.StringValue = actual;
                    spanToFind.Attributes[0].Key = "test";
                    whereSpanPropertyFilter.Attribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.EventAttribute:
                    spanToFind.Events[0].Attributes[0].Value.StringValue = actual;
                    spanToFind.Events[0].Attributes[0].Key = "test";
                    whereSpanPropertyFilter.EventAttribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.LinkAttribute:
                    spanToFind.Links[0].Attributes[0].Value.StringValue = actual;
                    spanToFind.Links[0].Attributes[0].Key = "test";
                    whereSpanPropertyFilter.LinkAttribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.InstrumentationScopeName:
                    scopeToFind.Scope.Name = actual;
                    whereSpanPropertyFilter.InstrumentationScopeName = stringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.InstrumentationScopeAttribute:
                    scopeToFind.Scope.Attributes[0].Value.StringValue = actual;
                    scopeToFind.Scope.Attributes[0].Key = "test";
                    whereSpanPropertyFilter.InstrumentationScopeAttribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.InstrumentationScopeVersion:
                    scopeToFind.Scope.Version = actual;
                    whereSpanPropertyFilter.InstrumentationScopeVersion = stringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.InstrumentationScopeSchemaUrl:
                    scopeToFind.SchemaUrl = actual;
                    whereSpanPropertyFilter.InstrumentationScopeSchemaUrl = stringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.ResourceSchemaUrl:
                    resourceToFind.SchemaUrl = actual;
                    whereSpanPropertyFilter.ResourceSchemaUrl = stringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.ResourceAttribute:
                    resourceToFind.Resource.Attributes[0].Value.StringValue = actual;
                    resourceToFind.Resource.Attributes[0].Key = "test";
                    whereSpanPropertyFilter.ResourceAttribute = new KeyValueProperty() { Key = "test", StringValue = stringProperty};
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
        [InlineData(
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.Equals, 
            WhereSpanPropertyFilter.PropertyOneofCase.SpanId, 
            true)
        ]
        [InlineData(
            new byte[]{0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.Equals, 
            WhereSpanPropertyFilter.PropertyOneofCase.SpanId, 
            false)
        ]
        [InlineData(
            new byte[]{0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.NotEquals, 
            WhereSpanPropertyFilter.PropertyOneofCase.SpanId, 
            true)
        ]
        [InlineData(
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.NotEquals, 
            WhereSpanPropertyFilter.PropertyOneofCase.SpanId, 
            false)
        ]
        [InlineData(
            new byte[]{}, 
            new byte[]{}, 
            ByteStringCompareAsType.Empty, 
            WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, 
            true)
        ]
        [InlineData(
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.Empty, 
            WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, 
            false)
        ]
        [InlineData(
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.NotEmpty, 
            WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, 
            true)
        ]
        [InlineData(
            new byte[]{}, 
            new byte[]{}, 
            ByteStringCompareAsType.NotEmpty, 
            WhereSpanPropertyFilter.PropertyOneofCase.ParentSpanId, 
            false)
        ]
        [InlineData(
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.Equals, 
            WhereSpanPropertyFilter.PropertyOneofCase.TraceId, 
            true)
        ]
        [InlineData(
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.Equals, 
            WhereSpanPropertyFilter.PropertyOneofCase.LinkTraceId, 
            true)
        ]
        [InlineData(
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.Equals, 
            WhereSpanPropertyFilter.PropertyOneofCase.LinkSpanId, 
            true)
        ]
        [InlineData(
            new byte[]{0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A}, 
            new byte[]{0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41}, 
            ByteStringCompareAsType.NotEquals, 
            WhereSpanPropertyFilter.PropertyOneofCase.Attribute, 
            true)
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
                case WhereSpanPropertyFilter.PropertyOneofCase.LinkTraceId:
                    spanToFind.Links[0].TraceId = ByteString.CopyFrom(actual);
                    whereSpanPropertyFilter.LinkTraceId = byteStringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.LinkSpanId:
                    spanToFind.Links[0].SpanId = ByteString.CopyFrom(actual);
                    whereSpanPropertyFilter.LinkSpanId = byteStringProperty;
                    break;
                case WhereSpanPropertyFilter.PropertyOneofCase.Attribute:
                    spanToFind.Attributes[0].Value.BytesValue = ByteString.CopyFrom(actual);
                    spanToFind.Attributes[0].Key = "test";
                    whereSpanPropertyFilter.Attribute = new KeyValueProperty()
                        { ByteStringValue = byteStringProperty, Key = "test" };
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
        // 3 traces are exported at 500, 1000, and 5000 ms. 
        [Theory]
        [InlineData(10, Duration.ValueOneofCase.SecondsValue, 3)] // Should return all traces
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