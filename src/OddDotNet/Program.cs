using OddDotNet;
using OddDotNet.Proto.Logs.V1;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Trace.V1;
using OddDotNet.Services;
using OddDotNet.Services.AppInsights;
using LogQueryService = OddDotNet.Services.LogQueryService;
using MetricQueryService = OddDotNet.Services.MetricQueryService;
using SpanQueryService = OddDotNet.Services.SpanQueryService;
// App Insights type aliases to avoid conflicts with OTEL types
using AiFlatRequest = OddDotNet.Proto.AppInsights.V1.FlatRequest;
using AiFlatDependency = OddDotNet.Proto.AppInsights.V1.FlatDependency;
using AiFlatException = OddDotNet.Proto.AppInsights.V1.FlatException;
using AiFlatTrace = OddDotNet.Proto.AppInsights.V1.FlatTrace;
using AiFlatEvent = OddDotNet.Proto.AppInsights.V1.FlatEvent;
using AiFlatMetric = OddDotNet.Proto.AppInsights.V1.FlatMetric;
using AiFlatPageView = OddDotNet.Proto.AppInsights.V1.FlatPageView;
using AiFlatAvailability = OddDotNet.Proto.AppInsights.V1.FlatAvailability;
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
builder.Services.AddScoped<SignalList<FlatMetric>>().AddScoped<ISignalList, SignalList<FlatMetric>>();
builder.Services.AddScoped<SignalList<FlatSpan>>().AddScoped<ISignalList, SignalList<FlatSpan>>();
builder.Services.AddScoped<SignalList<FlatLog>>().AddScoped<ISignalList, SignalList<FlatLog>>();
builder.Services.AddScoped<ChannelManager<FlatSpan>>();
builder.Services.AddScoped<ChannelManager<FlatMetric>>();
builder.Services.AddScoped<ChannelManager<FlatLog>>();

// App Insights SignalLists and ChannelManagers
builder.Services.AddScoped<SignalList<AiFlatRequest>>().AddScoped<ISignalList, SignalList<AiFlatRequest>>();
builder.Services.AddScoped<SignalList<AiFlatDependency>>().AddScoped<ISignalList, SignalList<AiFlatDependency>>();
builder.Services.AddScoped<SignalList<AiFlatException>>().AddScoped<ISignalList, SignalList<AiFlatException>>();
builder.Services.AddScoped<SignalList<AiFlatTrace>>().AddScoped<ISignalList, SignalList<AiFlatTrace>>();
builder.Services.AddScoped<SignalList<AiFlatEvent>>().AddScoped<ISignalList, SignalList<AiFlatEvent>>();
builder.Services.AddScoped<SignalList<AiFlatMetric>>().AddScoped<ISignalList, SignalList<AiFlatMetric>>();
builder.Services.AddScoped<SignalList<AiFlatPageView>>().AddScoped<ISignalList, SignalList<AiFlatPageView>>();
builder.Services.AddScoped<SignalList<AiFlatAvailability>>().AddScoped<ISignalList, SignalList<AiFlatAvailability>>();
builder.Services.AddScoped<ChannelManager<AiFlatRequest>>();
builder.Services.AddScoped<ChannelManager<AiFlatDependency>>();
builder.Services.AddScoped<ChannelManager<AiFlatException>>();
builder.Services.AddScoped<ChannelManager<AiFlatTrace>>();
builder.Services.AddScoped<ChannelManager<AiFlatEvent>>();
builder.Services.AddScoped<ChannelManager<AiFlatMetric>>();
builder.Services.AddScoped<ChannelManager<AiFlatPageView>>();
builder.Services.AddScoped<ChannelManager<AiFlatAvailability>>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<CacheCleanupBackgroundService>();

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