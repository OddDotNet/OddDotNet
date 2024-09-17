var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.OddDotNet>("odd")
    .WithHttpEndpoint(4317);
builder.Build().Run();
