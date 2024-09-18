var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.OddDotNet>("odd", "http");
builder.Build().Run();
