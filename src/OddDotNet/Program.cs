using OddDotNet;
using OddDotNet.Proto.Trace.V1;
using OddDotNet.Services;
using SpanQueryService = OddDotNet.Services.SpanQueryService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<ISignalList<FlatSpan>, SpanSignalList>().AddScoped<ISignalList, SpanSignalList>();
builder.Services.AddScoped<IChannelManager<FlatSpan>, SpanChannelManager>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<CacheCleanupBackgroundService>();

builder.Services.Configure<OddSettings>(options =>
{
    // We allow single and double underscores ('_', '__'). Single underscores
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