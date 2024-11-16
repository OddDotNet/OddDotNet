using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OddDotNet.Proto.Logs.V1;
using OddDotNet.Services;
using LogQueryService = OddDotNet.Proto.Logs.V1.LogQueryService;
using LogsService = OpenTelemetry.Proto.Collector.Logs.V1.LogsService;
using MetricQueryService = OddDotNet.Proto.Metrics.V1.MetricQueryService;
using MetricsService = OpenTelemetry.Proto.Collector.Metrics.V1.MetricsService;
using SpanQueryService = OddDotNet.Proto.Trace.V1.SpanQueryService;
using TraceService = OpenTelemetry.Proto.Collector.Trace.V1.TraceService;

namespace OddDotNet.Aspire.Tests;

public class AspireFixture : IAsyncLifetime
{
#pragma warning disable CS8618
    private TraceService.TraceServiceClient _traceServiceClient;
    private MetricsService.MetricsServiceClient _metricsServiceClient;
    private LogsService.LogsServiceClient _logsServiceClient;
    private SpanQueryService.SpanQueryServiceClient _spanQueryServiceClient;
    private MetricQueryService.MetricQueryServiceClient _metricQueryServiceClient;
    private LogQueryService.LogQueryServiceClient _logQueryServiceClient;
    private DistributedApplication _app;
#pragma warning disable CS8618
    
    public TraceService.TraceServiceClient TraceServiceClient => _traceServiceClient;
    
    public MetricsService.MetricsServiceClient MetricsServiceClient => _metricsServiceClient;
    public LogsService.LogsServiceClient LogsServiceClient => _logsServiceClient;
    public SpanQueryService.SpanQueryServiceClient SpanQueryServiceClient => _spanQueryServiceClient;
    
    public MetricQueryService.MetricQueryServiceClient MetricQueryServiceClient => _metricQueryServiceClient;

    public LogQueryService.LogQueryServiceClient LogQueryServiceClient => _logQueryServiceClient;
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
        
        var metricsServiceChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _metricsServiceClient = new MetricsService.MetricsServiceClient(metricsServiceChannel);
        
        var logsServiceChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _logsServiceClient = new LogsService.LogsServiceClient(logsServiceChannel);
            
        var spanQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _spanQueryServiceClient = new SpanQueryService.SpanQueryServiceClient(spanQueryChannel);
        
        var metricQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _metricQueryServiceClient = new MetricQueryService.MetricQueryServiceClient(metricQueryChannel);
        
        var logQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _logQueryServiceClient = new LogQueryService.LogQueryServiceClient(logQueryChannel);
    }

    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}