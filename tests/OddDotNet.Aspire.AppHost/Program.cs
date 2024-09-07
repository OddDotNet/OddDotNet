var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.OddDotNet_WebApi_One>("one");
builder.AddProject<Projects.OddDotNet_WebApi_Two>("two");
builder.AddProject<Projects.OddDotNet>("odd");

builder.Build().Run();
