using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace OddDotNet;

public class Class1
{
    public Class1()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddGrpc();
        var app = builder.Build();

        app.MapGrpcService<LogsService>();
        app.Urls.Add("http://localhost:907");
        app.StartAsync();
    }
}