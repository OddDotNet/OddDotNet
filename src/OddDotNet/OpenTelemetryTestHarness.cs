using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Logs.V1;
using OpenTelemetry.Proto.Metrics.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet;

public class OpenTelemetryTestHarness : IOpenTelemetryTestHarness
{
    public IAsyncTelemetryList<Span> Traces { get; }
    public IAsyncTelemetryList<LogRecord> Logs { get; }
    public IAsyncTelemetryList<Metric> Metrics { get; }
    public IAsyncTelemetryList<ExportTraceServiceRequest> ExportTraceServiceRequests { get; }

    // private readonly WebApplication _app;

    public OpenTelemetryTestHarness()
    {
        ExportTraceServiceRequests = new AsyncExportTraceServiceRequestTelemetryList();
        Traces = new AsyncTracesTelemetryList();
        Logs = new AsyncLogsTelemetryList();
        Metrics = new AsyncMetricsTelemetryList();
        
        // _app = ConfigureWebApplication();
    }

    // private WebApplication ConfigureWebApplication()
    // {
    //     var builder = WebApplication.CreateBuilder();
    //     builder.Services.AddGrpc();
    //     builder.Services.AddSingleton<IOpenTelemetryTestHarness>(this);
    //     builder.WebHost.ConfigureKestrel(options =>
    //     {
    //         options.Listen(IPAddress.Any, 4317, listenOptions =>
    //         {
    //             listenOptions.Protocols = HttpProtocols.Http2;
    //         });
    //     });
    //     
    //     var app = builder.Build();
    //     app.MapGrpcService<TracesService>();
    //     app.MapGrpcService<LogsService>();
    //     app.MapGrpcService<MetricsService>();
    //
    //     return app;
    // }

    // public async Task StartAsync(CancellationToken cancellationToken = default)
    // {
    //     await _app.StartAsync(cancellationToken);
    // }
    //
    // public async Task StopAsync(CancellationToken cancellationToken = default)
    // {
    //     await _app.StopAsync(cancellationToken);
    // }
}