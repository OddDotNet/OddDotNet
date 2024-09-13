using Google.Protobuf.Collections;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace OddDotNet.Aspire.Tests;

public class OddDotNetSpanQueryRequestTests
{
    public class SpanQueryRequestShould
    {
        [Fact]
        public async Task ReturnASpanQueryResponseWithTheCorrectSpanForOneWhereClause()
        {
            // Arrange
            var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();
            // ContainerResource oddProject = builder.Resources.OfType<ContainerResource>().First(r => r.Name == "odd");
            ProjectResource oddProject = builder.Resources.OfType<ProjectResource>().First(r => r.Name == "odd");
            ProjectResource oneProject = builder.Resources.OfType<ProjectResource>().First(r => r.Name == "one");
            
            await using var appHost = await builder.BuildAsync();
            
            var resourceNotificationService = appHost.Services.GetRequiredService<ResourceNotificationService>();
            await appHost.StartAsync();
            
            var oddEndPoint = oddProject.GetEndpoint("http");
            IResourceBuilder<ProjectResource> oneResourceBuilder = builder.CreateResourceBuilder(oneProject);
            oneResourceBuilder.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", oddEndPoint.Url);
            
            await resourceNotificationService.WaitForResourceAsync("odd", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(30));
            
            var httpClientForOne = appHost.CreateHttpClient("one");
            await resourceNotificationService.WaitForResourceAsync("one", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(30));
            
            var result = await httpClientForOne.GetAsync("/weatherforecast");
            Assert.True(result.IsSuccessStatusCode);
            
            var channel = GrpcChannel.ForAddress(oddEndPoint.Url);
            var clientSpanQueryService = new SpanQueryService.SpanQueryServiceClient(channel);
            
            var take = new Take
            {
                TakeExact = new TakeExact()
                {
                    Count = 1
                }
            }; 
            
            var whereFilter = new Where
             {
                 AttributeStringEqual = new WhereAttributeStringEqualFilter()
                 {
                     Attribute = "span.kind",
                     Compare = "server"
                 }
             };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, WhereFilters = { whereFilter }};
            
            // Act
            var reply = await clientSpanQueryService.QueryAsync(spanQueryRequest);
            // Assert
            
            Assert.NotEmpty(reply.Spans);
        }
    }
}