using Microsoft.OpenApi.Models;
using OddDotNet;
using OddDotNet.Proto.Logs.V1;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Trace.V1;
using OddDotNet.Services;
using LogQueryService = OddDotNet.Services.LogQueryService;
using MetricQueryService = OddDotNet.Services.MetricQueryService;
using SpanQueryService = OddDotNet.Services.SpanQueryService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// OpenAPI
// Not quite ready for prime-time, can't properly describe OpenTelemetry signals
// builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OddDotNet", Version = "v1" });
    c.CustomSchemaIds(x => x.FullName.Replace("+", "~"));
});


// SIGNALS
builder.Services.AddScoped<SignalList<FlatMetric>>().AddScoped<ISignalList, SignalList<FlatMetric>>();
builder.Services.AddScoped<SignalList<FlatSpan>>().AddScoped<ISignalList, SignalList<FlatSpan>>();
builder.Services.AddScoped<SignalList<FlatLog>>().AddScoped<ISignalList, SignalList<FlatLog>>();

// CHANNELS
builder.Services.AddScoped<ChannelManager<FlatSpan>>();
builder.Services.AddScoped<ChannelManager<FlatMetric>>();
builder.Services.AddScoped<ChannelManager<FlatLog>>();

builder.Services.AddSingleton(TimeProvider.System);

// BACKGROUND
builder.Services.AddHostedService<CacheCleanupBackgroundService>();

// CONFIGURATION
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
            endpoints.MapGrpcService<LogsService>();
            endpoints.MapGrpcService<MetricsService>();
            endpoints.MapGrpcService<TraceService>();
            endpoints.MapGrpcService<SpanQueryService>();
            endpoints.MapGrpcService<MetricQueryService>();
            endpoints.MapGrpcService<LogQueryService>();
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