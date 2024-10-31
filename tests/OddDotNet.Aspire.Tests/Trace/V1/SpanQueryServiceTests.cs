using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;
using SpanKindProperty = OddDotNet.Proto.Trace.V1.SpanKindProperty;
using SpanStatusCodeProperty = OddDotNet.Proto.Trace.V1.SpanStatusCodeProperty;

namespace OddDotNet.Aspire.Tests.Trace.V1;

public class SpanQueryServiceTests : IAsyncLifetime
{
    #pragma warning disable CS8618
    private TraceService.TraceServiceClient _traceServiceClient;
    private SpanQueryService.SpanQueryServiceClient _spanQueryServiceClient;
    private DistributedApplication _app;
    #pragma warning disable CS8618

    public class WhereSpanPropertyShould : SpanQueryServiceTests
    {
        [Fact]
        public async Task ReturnSpansWithMatchingStatusCodeProperty()
        {
            var request = TraceHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var codeToFind = spanToFind.Status.Code;

            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new WhereFilter()
            {
                Property = new WherePropertyFilter()
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
            Assert.Equal(spanToFind.SpanId, response.Spans[0].Span.SpanId);
        }
        
        [Fact]
        public async Task ReturnSpansWithMatchingKindProperty()
        {
            var request = TraceHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var codeToFind = spanToFind.Kind;

            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new WhereFilter()
            {
                Property = new WherePropertyFilter()
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
            Assert.Equal(spanToFind.SpanId, response.Spans[0].Span.SpanId);
        }
    }

    public class TakeAllShould : SpanQueryServiceTests
    {
        // 3 traces are exported at 500, 1000, and 5000 ms. 
        [Theory]
        [InlineData(10000, 3)] // Should return all traces
        [InlineData(1200, 2)] // Times out before 3rd trace is received
        [InlineData(60000, 3)] // should return all traces
        public async Task ReturnAllSpansWithinTimeframe(int takeDuration, int expectedCount)
        {
            var request = TraceHelpers.CreateExportTraceServiceRequest();
            var duration = new Duration
            {
                Milliseconds = takeDuration
            };
            
            var take = new Take
            {
                TakeAll = new TakeAll()
            };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, Duration = duration};
            
            // Start the query waiting for 3 seconds, and send spans at delayed intervals
            var responseTask = _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            var exportFirst = ExportDelayedTrace(request, TimeSpan.FromMilliseconds(250));
            var exportSecond = ExportDelayedTrace(request, TimeSpan.FromMilliseconds(500));
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

        var endpoint = _app.GetEndpoint("odd", "grpc");
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