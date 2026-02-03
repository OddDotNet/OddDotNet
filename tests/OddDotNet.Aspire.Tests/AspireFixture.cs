using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using LogQueryService = OddDotNet.Proto.Logs.V1.LogQueryService;
using LogsService = OpenTelemetry.Proto.Collector.Logs.V1.LogsService;
using MetricQueryService = OddDotNet.Proto.Metrics.V1.MetricQueryService;
using MetricsService = OpenTelemetry.Proto.Collector.Metrics.V1.MetricsService;
using SpanQueryService = OddDotNet.Proto.Trace.V1.SpanQueryService;
using TraceService = OpenTelemetry.Proto.Collector.Trace.V1.TraceService;

// App Insights Query Service aliases
using AiRequestQueryService = OddDotNet.Proto.AppInsights.V1.RequestQueryService;
using AiDependencyQueryService = OddDotNet.Proto.AppInsights.V1.DependencyQueryService;
using AiExceptionQueryService = OddDotNet.Proto.AppInsights.V1.ExceptionQueryService;
using AiTraceQueryService = OddDotNet.Proto.AppInsights.V1.TraceQueryService;
using AiEventQueryService = OddDotNet.Proto.AppInsights.V1.EventQueryService;
using AiMetricQueryService = OddDotNet.Proto.AppInsights.V1.MetricQueryService;
using AiPageViewQueryService = OddDotNet.Proto.AppInsights.V1.PageViewQueryService;
using AiAvailabilityQueryService = OddDotNet.Proto.AppInsights.V1.AvailabilityQueryService;

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
    
    // App Insights query service clients
    private AiRequestQueryService.RequestQueryServiceClient _aiRequestQueryServiceClient;
    private AiDependencyQueryService.DependencyQueryServiceClient _aiDependencyQueryServiceClient;
    private AiExceptionQueryService.ExceptionQueryServiceClient _aiExceptionQueryServiceClient;
    private AiTraceQueryService.TraceQueryServiceClient _aiTraceQueryServiceClient;
    private AiEventQueryService.EventQueryServiceClient _aiEventQueryServiceClient;
    private AiMetricQueryService.MetricQueryServiceClient _aiMetricQueryServiceClient;
    private AiPageViewQueryService.PageViewQueryServiceClient _aiPageViewQueryServiceClient;
    private AiAvailabilityQueryService.AvailabilityQueryServiceClient _aiAvailabilityQueryServiceClient;
    
    private DistributedApplication _app;
    private HttpClient _httpClient;
#pragma warning restore CS8618
    
    public TraceService.TraceServiceClient TraceServiceClient => _traceServiceClient;
    
    public MetricsService.MetricsServiceClient MetricsServiceClient => _metricsServiceClient;
    public LogsService.LogsServiceClient LogsServiceClient => _logsServiceClient;
    public SpanQueryService.SpanQueryServiceClient SpanQueryServiceClient => _spanQueryServiceClient;
    
    public MetricQueryService.MetricQueryServiceClient MetricQueryServiceClient => _metricQueryServiceClient;

    public LogQueryService.LogQueryServiceClient LogQueryServiceClient => _logQueryServiceClient;
    
    // App Insights query service client properties
    public AiRequestQueryService.RequestQueryServiceClient AiRequestQueryServiceClient => _aiRequestQueryServiceClient;
    public AiDependencyQueryService.DependencyQueryServiceClient AiDependencyQueryServiceClient => _aiDependencyQueryServiceClient;
    public AiExceptionQueryService.ExceptionQueryServiceClient AiExceptionQueryServiceClient => _aiExceptionQueryServiceClient;
    public AiTraceQueryService.TraceQueryServiceClient AiTraceQueryServiceClient => _aiTraceQueryServiceClient;
    public AiEventQueryService.EventQueryServiceClient AiEventQueryServiceClient => _aiEventQueryServiceClient;
    public AiMetricQueryService.MetricQueryServiceClient AiMetricQueryServiceClient => _aiMetricQueryServiceClient;
    public AiPageViewQueryService.PageViewQueryServiceClient AiPageViewQueryServiceClient => _aiPageViewQueryServiceClient;
    public AiAvailabilityQueryService.AvailabilityQueryServiceClient AiAvailabilityQueryServiceClient => _aiAvailabilityQueryServiceClient;
    
    public HttpClient HttpClient => _httpClient;
    
    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();
        _app = await builder.BuildAsync();
            
        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();

        await resourceNotificationService.WaitForResourceHealthyAsync("odd").WaitAsync(TimeSpan.FromSeconds(30));

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
        
        // App Insights query service clients
        var aiRequestQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _aiRequestQueryServiceClient = new AiRequestQueryService.RequestQueryServiceClient(aiRequestQueryChannel);
        
        var aiDependencyQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _aiDependencyQueryServiceClient = new AiDependencyQueryService.DependencyQueryServiceClient(aiDependencyQueryChannel);
        
        var aiExceptionQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _aiExceptionQueryServiceClient = new AiExceptionQueryService.ExceptionQueryServiceClient(aiExceptionQueryChannel);
        
        var aiTraceQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _aiTraceQueryServiceClient = new AiTraceQueryService.TraceQueryServiceClient(aiTraceQueryChannel);
        
        var aiEventQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _aiEventQueryServiceClient = new AiEventQueryService.EventQueryServiceClient(aiEventQueryChannel);
        
        var aiMetricQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _aiMetricQueryServiceClient = new AiMetricQueryService.MetricQueryServiceClient(aiMetricQueryChannel);
        
        var aiPageviewQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _aiPageViewQueryServiceClient = new AiPageViewQueryService.PageViewQueryServiceClient(aiPageviewQueryChannel);
        
        var aiAvailabilityQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _aiAvailabilityQueryServiceClient = new AiAvailabilityQueryService.AvailabilityQueryServiceClient(aiAvailabilityQueryChannel);
        
        // HTTP client for REST endpoints (like /v2/track)
        var httpEndpoint = _app.GetEndpoint("odd", "http");
        _httpClient = new HttpClient { BaseAddress = new Uri(httpEndpoint.AbsoluteUri) };
    }

    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}