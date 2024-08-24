# OddDotNet

OddDotNet stands for Observability Driven Development (ODD) in .NET.

## Description
OddDotNet is a test harness for ODD. It includes an OTLP receiver that supports either http or grpc, following
traditional OpenTelemetry Collector standards.

## Quickstart
### OTLP gRPC Exporter
The OddDotNet test harness can be used to receive and process gRPC logs, traces, and metrics.

#### Program.cs
```csharp
[Fact]
public async Task ExampleConfigurationTest()
{
    await using var provider = new ServiceCollection()
        .AddYourBusinessServices()
        .AddOpenTelemetryTestHarness(x =>
            {
                x.UsingHttp();
                x.Timeout = TimeSpan.FromSeconds(30);
            }
        ).BuildServiceProvider(true);

    var someService = provider.GetRequiredService<ISomeService>();
    var harness = provider.GetRequiredService<IOpenTelemetryTestHarness>();

    await harness.Start();

    someService.GenerateTelemetry();

    var span = await harness.Received.FirstOrDefault(x => x.Attribute["service.id"] == "Service1");

    Assert.NotNull(span);
```

or

```csharp
[Fact]
public async Task ExampleWithWebAppFactory()
{
    await using var application = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Optionally throw exception if OpenTelemetry is not configured
                services.AddOpenTelemetryTestHarness(throwOnMissingConfiguration: true);
            }
        });

    var harness = application.Services.GetOpenTelemetryTestHarness();

    using var client = application.CreateClient();

    await client.GetAsync("/");

    var span  = await harness.Received.FirstOrDefault(x => x.Attribute["service.id"] == "Service1");

    Assert.NotNull(span);
```

or

```csharp
[Fact]
public async Task ExampleWithDistributedApplicationTestBuilder()
{
    var appHost = await DistributedApplicationTestBuilder.CreateAsync<Projects.AppHost>();

    appHost.AddOpenTelemetryTestHarness();

    await using var app = await appHost.BuildAsync();

    await app.StartAsync();

    var harness = app.Services.GetOpenTelemetryTestHarness();

    using var client = app.CreateHttpClient("frontend");

    await client.GetAsync("/");

    var span  = await harness.Received.FirstOrDefault(x => x.Attribute["service.id"] == "Service1");

    Assert.NotNull(span);
}
```

## Tools and Setup
This repository makes use of `git submodule`s to clone down the proto files from GitHub, located
[here](https://github.com/open-telemetry/opentelemetry-proto).

When cloning down the repo, you'll need to `git clone --recurse-submodules` to pull in the proto
file git repo.