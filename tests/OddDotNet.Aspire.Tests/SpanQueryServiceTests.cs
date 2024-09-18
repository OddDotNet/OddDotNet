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
    
    public class WhereAttributeExistsShould : SpanQueryServiceTests
    {
        [Fact]
        public async Task ReturnSpanWithMatchingAttribute()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var take = new Take
            {
                TakeExact = new TakeExact()
                {
                    Count = 1
                }
            }; 
            
            var whereFilter = new Where
            {
                AttributeExists = new WhereAttributeExistsFilter()
                {
                    Attribute = request.ResourceSpans[0].ScopeSpans[0].Spans[0].Attributes[0].Key,
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, WhereFilters = { whereFilter }};
            
            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(request.ResourceSpans[0].ScopeSpans[0].Spans[0].SpanId, response.Spans[0].SpanId);
        }
    }

    public class WhereAttributeStringEqualShould : SpanQueryServiceTests
    {
        [Fact]
        public async Task ReturnSpanWithMatchingAttribute()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var take = new Take
            {
                TakeExact = new TakeExact()
                {
                    Count = 1
                }
            };

            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var attributeToFind = spanToFind.Attributes[0];
            var whereFilter = new Where
            {
                AttributeStringEqual = new WhereAttributeStringEqualFilter()
                {
                    Attribute = attributeToFind.Key,
                    Compare = attributeToFind.Value.StringValue
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, WhereFilters = { whereFilter }};
            
            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }
    }

    public class WhereAttributeIntEqualShould : SpanQueryServiceTests
    {
        [Fact]
        public async Task ReturnSpanWithMatchingAttribute()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var intAttribute = TestHelpers.CreateKeyValue("test.int", 123);
            request.ResourceSpans[0].ScopeSpans[0].Spans[0].Attributes.Add(intAttribute);
            await _traceServiceClient.ExportAsync(request);
            
            var take = new Take
            {
                TakeExact = new TakeExact()
                {
                    Count = 1
                }
            };

            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var whereFilter = new Where
            {
                AttributeIntEqual = new WhereAttributeIntEqualFilter()
                {
                    Attribute = intAttribute.Key,
                    Compare = intAttribute.Value.IntValue
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, WhereFilters = { whereFilter }};
            
            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
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
        _traceServiceClient.ExportAsync(request);
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