using OddDotNet;
using OddDotNet.Services;
using SpanQueryService = OddDotNet.Services.SpanQueryService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddScoped<ISignalList<Span>, SpanSignalList>();
builder.Services.AddScoped<IChannelManager<Span>, SpanChannelManager>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LogsService>();
app.MapGrpcService<MetricsService>();
app.MapGrpcService<TraceService>();
app.MapGrpcService<SpanQueryService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.Run();