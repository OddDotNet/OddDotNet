var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OddDotNet_WebApi_One>("one");
builder.AddProject<Projects.OddDotNet_WebApi_Two>("two");
//builder.AddProject<Projects.OddDotNet>("odd");
builder.AddDockerfile("odd", "../../src/OddDotNet/")
    .WithHttpEndpoint(4317,4317);
builder.Build().Run();
