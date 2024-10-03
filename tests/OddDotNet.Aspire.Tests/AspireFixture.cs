using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OddDotNet.Proto.Spans.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace OddDotNet.Aspire.Tests;

public class AspireFixture : IAsyncLifetime
{
#pragma warning disable CS8618
    private TraceService.TraceServiceClient _traceServiceClient;
    private SpanQueryService.SpanQueryServiceClient _spanQueryServiceClient;
    private DistributedApplication _app;
#pragma warning disable CS8618
    
    public TraceService.TraceServiceClient TraceServiceClient => _traceServiceClient;
    public SpanQueryService.SpanQueryServiceClient SpanQueryServiceClient => _spanQueryServiceClient;

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