using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Proto.Collector.Trace.V1;

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

    public class ExportTraceServiceRequests
    {
        public class AnyAsyncShould
        {
            [Fact]
            public async Task ReturnTrueWhenAExportTraceServiceRequestIsReceivedThatMatchesTheFilterDelegate()
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
                
                FilterDelegate<ExportTraceServiceRequest> filterDelegate = (context) =>
                {
                    return context.ResourceSpans.FirstOrDefault(rs =>
                        rs.ScopeSpans.FirstOrDefault(sp =>
                            sp.Spans.FirstOrDefault(span => span.Name == "GET /weatherforecast") != null) !=
                        null) != null;
                };
                
                bool resultUsingExplicitFilterDelegate = await harness.ExportTraceServiceRequests.AnyAsync(filterDelegate);
                Assert.True(resultUsingExplicitFilterDelegate);
                
                //***Can also use lambda which is more likely to be used in practice***
                // var resultUsingLambdaFilter = await harness.ExportTraceServiceRequests.AnyAsync((context =>
                // {
                //     return context.ResourceSpans.FirstOrDefault(rs =>
                //         rs.ScopeSpans.FirstOrDefault(sp =>
                //             sp.Spans.FirstOrDefault(span => span.Name == "GET /weatherforecast") != null) !=
                //         null) != null;
                // }));
            }
        }
        
        public class FirstAsyncShould
        {
            [Fact]
            public async Task ReturnExportTraceServiceRequesteWhenAExportTraceServiceRequestIsReceivedThatMatchesTheFilterDelegate()
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
                
                FilterDelegate<ExportTraceServiceRequest> filterDelegate = (context) =>
                {
                    return context.ResourceSpans.FirstOrDefault(rs =>
                        rs.ScopeSpans.FirstOrDefault(sp =>
                            sp.Spans.FirstOrDefault(span => span.Name == "GET /weatherforecast") != null) !=
                        null) != null;
                };
                
                ExportTraceServiceRequest exportTraceServiceRequest = await harness.ExportTraceServiceRequests.FirstAsync(filterDelegate);
                
                Assert.True(exportTraceServiceRequest.ResourceSpans.FirstOrDefault(rs =>
                    rs.ScopeSpans.FirstOrDefault(sp =>
                        sp.Spans.FirstOrDefault(span => span.Name == "GET /weatherforecast") != null) !=
                    null) != null);
            }
        }
    }
}