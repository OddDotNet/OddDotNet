using OddDotNet;
using OddDotNet.Proto.Logs.V1;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Trace.V1;
using OddDotNet.Services;
using OddDotNet.Services.AppInsights;
using OddDotNet.Services.Query;
using LogQueryService = OddDotNet.Services.LogQueryService;
using MetricQueryService = OddDotNet.Services.MetricQueryService;
using SpanQueryService = OddDotNet.Services.SpanQueryService;
// App Insights type aliases to avoid conflicts with OTEL types
using AiFlatRequest = OddDotNet.Proto.AppInsights.V1.Request.FlatRequest;
using AiFlatDependency = OddDotNet.Proto.AppInsights.V1.Dependency.FlatDependency;
using AiFlatException = OddDotNet.Proto.AppInsights.V1.Exception.FlatException;
using AiFlatTrace = OddDotNet.Proto.AppInsights.V1.Trace.FlatTrace;
using AiFlatEvent = OddDotNet.Proto.AppInsights.V1.Event.FlatEvent;
using AiFlatMetric = OddDotNet.Proto.AppInsights.V1.Metric.FlatMetric;
using AiFlatPageView = OddDotNet.Proto.AppInsights.V1.PageView.FlatPageView;
using AiFlatAvailability = OddDotNet.Proto.AppInsights.V1.Availability.FlatAvailability;
// App Insights query service aliases
using AiRequestQueryService = OddDotNet.Services.AppInsights.RequestQueryService;
using AiDependencyQueryService = OddDotNet.Services.AppInsights.DependencyQueryService;
using AiExceptionQueryService = OddDotNet.Services.AppInsights.ExceptionQueryService;
using AiTraceQueryService = OddDotNet.Services.AppInsights.TraceQueryService;
using AiEventQueryService = OddDotNet.Services.AppInsights.EventQueryService;
using AiMetricQueryService = OddDotNet.Services.AppInsights.MetricQueryService;
using AiPageViewQueryService = OddDotNet.Services.AppInsights.PageViewQueryService;
using AiAvailabilityQueryService = OddDotNet.Services.AppInsights.AvailabilityQueryService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// OTEL SignalLists and ChannelManagers
builder.Services.AddSingleton<SignalList<FlatMetric>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<FlatMetric>>());
builder.Services.AddSingleton<SignalList<FlatSpan>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<FlatSpan>>());
builder.Services.AddSingleton<SignalList<FlatLog>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<FlatLog>>());
builder.Services.AddSingleton<ChannelManager<FlatSpan>>();
builder.Services.AddSingleton<ChannelManager<FlatMetric>>();
builder.Services.AddSingleton<ChannelManager<FlatLog>>();

// App Insights SignalLists and ChannelManagers
builder.Services.AddSingleton<SignalList<AiFlatRequest>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<AiFlatRequest>>());
builder.Services.AddSingleton<SignalList<AiFlatDependency>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<AiFlatDependency>>());
builder.Services.AddSingleton<SignalList<AiFlatException>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<AiFlatException>>());
builder.Services.AddSingleton<SignalList<AiFlatTrace>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<AiFlatTrace>>());
builder.Services.AddSingleton<SignalList<AiFlatEvent>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<AiFlatEvent>>());
builder.Services.AddSingleton<SignalList<AiFlatMetric>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<AiFlatMetric>>());
builder.Services.AddSingleton<SignalList<AiFlatPageView>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<AiFlatPageView>>());
builder.Services.AddSingleton<SignalList<AiFlatAvailability>>().AddSingleton<ISignalList>(sp => sp.GetRequiredService<SignalList<AiFlatAvailability>>());
builder.Services.AddSingleton<ChannelManager<AiFlatRequest>>();
builder.Services.AddSingleton<ChannelManager<AiFlatDependency>>();
builder.Services.AddSingleton<ChannelManager<AiFlatException>>();
builder.Services.AddSingleton<ChannelManager<AiFlatTrace>>();
builder.Services.AddSingleton<ChannelManager<AiFlatEvent>>();
builder.Services.AddSingleton<ChannelManager<AiFlatMetric>>();
builder.Services.AddSingleton<ChannelManager<AiFlatPageView>>();
builder.Services.AddSingleton<ChannelManager<AiFlatAvailability>>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<CacheCleanupBackgroundService>();

// Unified /query/v1/* REST surface
builder.Services.AddQuerySurface();

builder.Services.Configure<OddSettings>(options =>
{
    // Allow single and double underscores ('_', '__'). Single underscores
    // don't automatically map, so check for them explicitly.
    // eg. ODD__CACHE__EXPIRATION and ODD_CACHE_EXPIRATION are both valid
    var cacheExpirationEnvVarValue = Environment.GetEnvironmentVariable(OddSettings.CacheExpirationEnvVarName);
    if (!string.IsNullOrEmpty(cacheExpirationEnvVarValue))
        options.Cache.Expiration = uint.Parse(cacheExpirationEnvVarValue);
    
    var cacheCleanupIntervalEnvVarValue = Environment.GetEnvironmentVariable(OddSettings.CacheCleanupIntervalEnvVarName);
    if (!string.IsNullOrEmpty(cacheCleanupIntervalEnvVarValue))
        options.Cache.CleanupInterval = uint.Parse(cacheCleanupIntervalEnvVarValue);
});

var app = builder.Build();

// Configure `application/grpc` requests to map to the grpc services
app.MapWhen(context => context.Request.ContentType == "application/grpc", iab =>
{
    iab.UseRouting()
        .UseEndpoints(endpoints =>
        {
            // OTEL services
            endpoints.MapGrpcService<LogsService>();
            endpoints.MapGrpcService<MetricsService>();
            endpoints.MapGrpcService<TraceService>();
            endpoints.MapGrpcService<SpanQueryService>();
            endpoints.MapGrpcService<MetricQueryService>();
            endpoints.MapGrpcService<LogQueryService>();
            
            // App Insights query services
            endpoints.MapGrpcService<AiRequestQueryService>();
            endpoints.MapGrpcService<AiDependencyQueryService>();
            endpoints.MapGrpcService<AiExceptionQueryService>();
            endpoints.MapGrpcService<AiTraceQueryService>();
            endpoints.MapGrpcService<AiEventQueryService>();
            endpoints.MapGrpcService<AiMetricQueryService>();
            endpoints.MapGrpcService<AiPageViewQueryService>();
            endpoints.MapGrpcService<AiAvailabilityQueryService>();
        });
});

// Configure all other content types to map to http controllers
app.MapWhen(context => context.Request.ContentType != "application/grpc", iab =>
{
    iab.UseRouting()
        .UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/healthz");
        });
    
    if (app.Environment.IsDevelopment())
    {
        iab.UseSwagger();
        iab.UseSwaggerUI();
    }
});

app.Run();