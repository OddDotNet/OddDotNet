using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace OddDotNet.Aspire.Tests.Trace.V1;

public class SpanTakeTests : IAsyncLifetime
{
#pragma warning disable CS8618
    private TraceService.TraceServiceClient _traceServiceClient;
    private SpanQueryService.SpanQueryServiceClient _spanQueryServiceClient;
    private DistributedApplication _app;
#pragma warning disable CS8618

    public class TakeFirstShould : SpanTakeTests
    {
        [Fact]
        public async Task TakeTheFirstMatchingTrace()
        {
            var request = TraceHelpers.CreateExportTraceServiceRequest();
            var take = new Take
            {
                TakeFirst = new()
            };

            await _traceServiceClient.ExportAsync(request);

            var response = await _spanQueryServiceClient.QueryAsync(new SpanQueryRequest { Take = take });
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(request.ResourceSpans[0].ScopeSpans[0].Spans[0].SpanId, response.Spans[0].Span.SpanId);
        }
    }

    public class TakeExactShould : SpanTakeTests
    {
        [Fact]
        public async Task ReturnMatchingSpansUpToCount()
        {
            var requestOne = TraceHelpers.CreateExportTraceServiceRequest();
            var requestTwo = TraceHelpers.CreateExportTraceServiceRequest();
            var requestThree = TraceHelpers.CreateExportTraceServiceRequest();

            var take = new Take
            {
                TakeExact = new TakeExact { Count = 2 }
            };
            
            await _traceServiceClient.ExportAsync(requestOne);
            await _traceServiceClient.ExportAsync(requestTwo);
            await _traceServiceClient.ExportAsync(requestThree);
            
            var response = await _spanQueryServiceClient.QueryAsync(new SpanQueryRequest { Take = take });
            
            Assert.Equal(2, response.Spans.Count);
        }
    }
    
    public class TakeAllShould : SpanTakeTests
    {
        [Fact]
        public async Task ReturnMatchingTracesWithinTimeframe()
        {
            var requestOne = TraceHelpers.CreateExportTraceServiceRequest();
            var requestTwo = TraceHelpers.CreateExportTraceServiceRequest();
            var requestThree = TraceHelpers.CreateExportTraceServiceRequest();

            var take = new Take
            {
                TakeAll = new()
            };

            var durationOne = new Duration
            {
                Milliseconds = 1000
            };
            
            var durationTwo = new Duration
            {
                Milliseconds = 3000
            };
            
            var durationThree = new Duration
            {
                Milliseconds = 5000
            };
            
            var queryOneTask = _spanQueryServiceClient.QueryAsync(new SpanQueryRequest { Take = take, Duration = durationOne });
            var queryTwoTask = _spanQueryServiceClient.QueryAsync(new SpanQueryRequest { Take = take, Duration = durationTwo });
            var queryThreeTask = _spanQueryServiceClient.QueryAsync(new SpanQueryRequest { Take = take, Duration = durationThree });
            var exportOne = ExportDelayedTrace(requestOne, TimeSpan.Zero);
            var exportTwo = ExportDelayedTrace(requestTwo, TimeSpan.FromMilliseconds(2000));
            var exportThree = ExportDelayedTrace(requestThree, TimeSpan.FromMilliseconds(4000));

            await Task.WhenAll(
                queryOneTask.ResponseAsync, 
                queryTwoTask.ResponseAsync, 
                queryThreeTask.ResponseAsync, 
                exportOne, 
                exportTwo, 
                exportThree);
            
            var responseOne = await queryOneTask;
            Assert.Single(responseOne.Spans);
            
            var responseTwo = await queryTwoTask;
            Assert.Equal(2, responseTwo.Spans.Count);
            
            var responseThree = await queryThreeTask;
            Assert.Equal(3, responseThree.Spans.Count);
            Assert.Equal(requestThree.ResourceSpans[0].ScopeSpans[0].Spans[0].SpanId, responseThree.Spans[2].Span.SpanId);
        }
    }
    // Need a separate "AspireFixture" here as we need to modify the env vars of the project before starting.
    public async Task InitializeAsync()
    {
        const string oddResource = "odd";
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();
        builder
            .CreateResourceBuilder(builder
                .Resources
                .First(resource => resource.Name == oddResource))
            .WithAnnotation(new EnvironmentCallbackAnnotation("ODD_CACHE_EXPIRATION", () => "1000"))
            .WithAnnotation(new EnvironmentCallbackAnnotation("ODD_CACHE_CLEANUP_INTERVAL", () => "1000"));
        _app = await builder.BuildAsync();
            
        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();

        await resourceNotificationService.WaitForResourceAsync(oddResource).WaitAsync(TimeSpan.FromSeconds(30));

        var endpoint = _app.GetEndpoint(oddResource, "grpc");
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
    
    private async Task ExportDelayedTrace(ExportTraceServiceRequest request, TimeSpan delay)
    {
        await Task.Delay(delay);
        await _traceServiceClient.ExportAsync(request);
    }
}