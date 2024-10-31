var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.OddDotNet>("odd");
// builder.AddDockerfile("odd", "../../src/OddDotNet")
//     .WithHttpEndpoint(targetPort: 4317, name: "grpc")
//     .WithHttpEndpoint(targetPort: 4318, name: "http");
builder.Build().Run();
