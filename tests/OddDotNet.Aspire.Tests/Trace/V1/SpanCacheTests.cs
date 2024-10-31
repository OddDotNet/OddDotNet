using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace OddDotNet.Aspire.Tests.Trace.V1;

public class SpanCacheTests : IAsyncLifetime
{
#pragma warning disable CS8618
    private TraceService.TraceServiceClient _traceServiceClient;
    private SpanQueryService.SpanQueryServiceClient _spanQueryServiceClient;
    private DistributedApplication _app;
#pragma warning disable CS8618
    [Fact]
    public async Task RemoveSpansAfterConfiguredTimeout()
    {
        // Arrange
        var exportRequest = TraceHelpers.CreateExportTraceServiceRequest();
        var traceId = exportRequest.ResourceSpans[0].ScopeSpans[0].Spans[0].TraceId;
        var take = new Take { TakeFirst = new() };
        var duration = new Duration { Milliseconds = 1000 };
        var traceIdFilter = new WhereFilter
        {
            Property = new WherePropertyFilter
            {
                TraceId = new ByteStringProperty
                {
                    Compare = traceId,
                    CompareAs = ByteStringCompareAsType.Equals
                }
            }
        };
        var spanQueryRequest = new SpanQueryRequest { Take = take, Duration = duration, Filters = { traceIdFilter } };
        
        // ACT
        await _traceServiceClient.ExportAsync(exportRequest);
        var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
        
        // The first response should contain the span since it hasn't yet been deleted
        Assert.NotEmpty(response.Spans);

        // Give the background service time to clear the cache
        await Task.Delay(1000);
        
        // The second response should be empty as the span should have been cleared from cache.
        response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
        Assert.Empty(response.Spans);
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
            .WithAnnotation(new EnvironmentCallbackAnnotation("ODD_CACHE_EXPIRATION", () => "250"))
            .WithAnnotation(new EnvironmentCallbackAnnotation("ODD_CACHE_CLEANUP_INTERVAL", () => "500"));
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
}