using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace OddDotNet;

public static class DistributedApplicationTestingBuilderExtensions
{
    public static void AddOpenTelemetryTestHarness(this IDistributedApplicationTestingBuilder builder,
        bool throwOnMissingConfiguration = false)
    {
        IEnumerable<ProjectResource> projects = builder.Resources.OfType<ProjectResource>();
        foreach (var project in projects)
        {
            IResourceBuilder<ProjectResource> resourceBuilder = builder.CreateResourceBuilder(project);

            resourceBuilder.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317");
        }
        
        builder.Services.AddSingleton<IOpenTelemetryTestHarness, OpenTelemetryTestHarness>();
        builder.Services.AddHostedService<OpenTelemetryBackgroundService>();
        
    }
}