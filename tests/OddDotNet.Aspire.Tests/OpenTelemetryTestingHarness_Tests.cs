using Microsoft.Extensions.DependencyInjection;

namespace OddDotNet.Aspire.Tests;

public class OpenTelemetryTestingHarness_Tests
{
    public class Traces
    {
        public class AnyAsyncShould
        {
            [Fact]
            public async Task ReturnTrueWhenAnyTraceReceived()
            {
                var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();

                builder.AddOpenTelemetryTestHarness();

                builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
                {
                    clientBuilder.AddStandardResilienceHandler();
                });
                
                await using var app = await builder.BuildAsync();
                
                var harness = app.Services.GetRequiredService<IOpenTelemetryTestHarness>();

                var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
                
                await app.StartAsync();

                var httpClient = app.CreateHttpClient("one");

                await resourceNotificationService.WaitForResourceAsync("one", KnownResourceStates.Running)
                    .WaitAsync(TimeSpan.FromSeconds(30));
                
                // ACT
                await httpClient.GetAsync("/weatherforecast");

                var result = await harness.Metrics.AnyAsync();
                Assert.True(result);
            }
        }
    }
}