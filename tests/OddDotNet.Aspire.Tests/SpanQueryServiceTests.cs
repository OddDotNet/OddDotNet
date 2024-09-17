using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Trace.V1;
using OtelResource = OpenTelemetry.Proto.Resource.V1.Resource;
using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OtelAnyValue = OpenTelemetry.Proto.Common.V1.AnyValue;

namespace OddDotNet.Aspire.Tests;

public class SpanQueryServiceTests
{
    public class WhereAttributeExistsShould()
    {
        [Fact]
        public async Task ReturnSpanWithMatchingAttributeWhenAttributeExists()
        {
            OtelAnyValue attributeValue = new OtelAnyValue()
            {
                StringValue = "test"
            };
            KeyValue attribute = new KeyValue()
            {
                Key = "test",
                Value = attributeValue
            };
            OtelSpan span = new OtelSpan();
            span.Attributes.Add(attribute);
            ScopeSpans scopeSpan = new ScopeSpans();
            scopeSpan.Spans.Add(span);
            
            OtelResource resource = new OtelResource();
            resource.DroppedAttributesCount = 0;
            ResourceSpans resourceSpan = new ResourceSpans();
            resourceSpan.Resource = resource;
            resourceSpan.ScopeSpans.Add(scopeSpan);
            ExportTraceServiceRequest request = new ExportTraceServiceRequest();
            request.ResourceSpans.Add(resourceSpan);

            var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();
            await using var app = await builder.BuildAsync();
            
            var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
            await app.StartAsync();

            await resourceNotificationService.WaitForResourceAsync("odd").WaitAsync(TimeSpan.FromSeconds(30));

            var endpoint = app.GetEndpoint("odd", "http");
            var traceServiceChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
            TraceService.TraceServiceClient traceServiceClient = new TraceService.TraceServiceClient(traceServiceChannel);

            await traceServiceClient.ExportAsync(request);
            
            var spanQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
            SpanQueryService.SpanQueryServiceClient spanQueryServiceClient = new SpanQueryService.SpanQueryServiceClient(spanQueryChannel);
            
            var take = new Take
            {
                TakeExact = new TakeExact()
                {
                    Count = 1
                }
            }; 
            
            var whereFilter = new Where
            {
                AttributeExists = new WhereAttributeExistsFilter()
                {
                    Attribute = "test",
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, WhereFilters = { whereFilter }};
            
            var response = await spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
        }
    }
}