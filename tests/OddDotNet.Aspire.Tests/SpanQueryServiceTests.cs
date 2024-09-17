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
            var request = TestHelpers.CreateExportTraceServiceRequest();

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
                    Attribute = request.ResourceSpans[0].ScopeSpans[0].Spans[0].Attributes[0].Key,
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, WhereFilters = { whereFilter }};
            
            var response = await spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(request.ResourceSpans[0].ScopeSpans[0].Spans[0].SpanId, response.Spans[0].SpanId);
        }
    }

    public class WhereAttributeStringEqualShould
    {
        [Fact]
        public async Task ReturnSpanWithMatchingAttributeWhenAttributeIsEqual()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();

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

            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var attributeToFind = spanToFind.Attributes[0];
            var whereFilter = new Where
            {
                AttributeStringEqual = new WhereAttributeStringEqualFilter()
                {
                    Attribute = attributeToFind.Key,
                    Compare = attributeToFind.Value.StringValue
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, WhereFilters = { whereFilter }};
            
            var response = await spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }
    }

    public class WhereAttributeIntEqualShould
    {
        [Fact]
        public async Task ReturnSpanWithMatchingAttributeWhenAttributeIsEqual()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var intAttribute = TestHelpers.CreateKeyValue("test.int", 123);
            request.ResourceSpans[0].ScopeSpans[0].Spans[0].Attributes.Add(intAttribute);

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

            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var whereFilter = new Where
            {
                AttributeIntEqual = new WhereAttributeIntEqualFilter()
                {
                    Attribute = intAttribute.Key,
                    Compare = intAttribute.Value.IntValue
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest { Take = take, WhereFilters = { whereFilter }};
            
            var response = await spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }
    }
}