using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OddDotNet;

public class OpenTelemetryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;

    public OpenTelemetryBackgroundService(IServiceProvider serviceProvider)
    {
        _services = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var testHarness = scope.ServiceProvider.GetRequiredService<IOpenTelemetryTestHarness>();
        
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(testHarness);
        builder.Services.AddGrpc();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Any, 4317, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            } );
        });
        var app = builder.Build();
        app.MapGrpcService<LogsService>();
        app.MapGrpcService<TracesService>();
        app.MapGrpcService<MetricsService>();
            
        await app.StartAsync(stoppingToken);
    }
}