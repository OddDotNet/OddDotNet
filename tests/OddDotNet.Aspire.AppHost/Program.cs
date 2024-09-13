var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OddDotNet_WebApi_One>("one", launchProfileName: "http")
    .WithEnvironment("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", "http://localhost:4317");
builder.AddProject<Projects.OddDotNet_WebApi_Two>("two");
// builder.AddProject<Projects.OddDotNet>("odd")
//     .WithHttpEndpoint(port: 4317, targetPort: 4317, isProxied: false);
builder.AddDockerfile("odd", "../../src/OddDotNet/")
    .WithHttpEndpoint(4317,4317, isProxied: false);
builder.Build().Run();
