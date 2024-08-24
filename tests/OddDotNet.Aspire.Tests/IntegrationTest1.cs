using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OddDotNet.Aspire.Tests.Tests;

public class IntegrationTest1
{
    // Instructions:
    // 1. Add a project reference to the target AppHost project, e.g.:
    //
    //    <ItemGroup>
    //        <ProjectReference Include="../MyAspireApp.AppHost/MyAspireApp.AppHost.csproj" />
    //    </ItemGroup>
    //
    // 2. Uncomment the following example test and update 'Projects.MyAspireApp_AppHost' to match your AppHost project:
    // 
    // [Fact]
    // public async Task GetWebResourceRootReturnsOkStatusCode()
    // {
    //     // Arrange
    //     var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MyAspireApp_AppHost>();
    //     await using var app = await appHost.BuildAsync();
    //     await app.StartAsync();

    //     // Act
    //     var httpClient = app.CreateHttpClient("webfrontend");
    //     var response = await httpClient.GetAsync("/");

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    // }

    [Fact]
    public async Task DoTheThing()
    {
        var webBuilder = WebApplication.CreateBuilder();
        webBuilder.Services.AddGrpc();
        var webApp = webBuilder.Build();
        webApp.Urls.Add("http://localhost:4317");
        webApp.MapGrpcService<LogsService>();
        await webApp.StartAsync();
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();
        
        foreach (var resource in appHost.Resources)
        {
            var builder = appHost.CreateResourceBuilder(resource);
        }
        await using var app = await appHost.BuildAsync();
        
        await app.StartAsync();
    }
}