using OddDotNet;
using OddDotNet.Proto.Spans.V1;
using OddDotNet.Services;
using SpanQueryService = OddDotNet.Services.SpanQueryService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<ISignalList<Span>, SpanSignalList>();
builder.Services.AddScoped<IChannelManager<Span>, SpanChannelManager>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
});

app.Run();