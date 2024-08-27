using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OddDotNet;

public class OpenTelemetryBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddGrpc();
        var app = builder.Build();
        app.Urls.Add("http://localhost:4317");
        app.MapGrpcService<LogsService>();
        app.MapGrpcService<TracesService>();
        
        await app.StartAsync(stoppingToken);
    }
}