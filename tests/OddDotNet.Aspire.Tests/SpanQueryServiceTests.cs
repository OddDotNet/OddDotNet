using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Proto.Collector.Trace.V1;

namespace OddDotNet.Aspire.Tests;

public class SpanQueryServiceTests : IAsyncLifetime
{
    #pragma warning disable CS8618
    private TraceService.TraceServiceClient _traceServiceClient;
    private SpanQueryService.SpanQueryServiceClient _spanQueryServiceClient;
    private DistributedApplication _app;
    #pragma warning disable CS8618
    
    public class WhereAttributeExistsShould : SpanQueryServiceTests
    {
        [Fact]
        public async Task ReturnSpanWithMatchingAttribute()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
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
            
            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(request.ResourceSpans[0].ScopeSpans[0].Spans[0].SpanId, response.Spans[0].SpanId);
        }
    }

    public class WhereAttributeStringEqualShould : SpanQueryServiceTests
    {
        [Fact]
        public async Task ReturnSpanWithMatchingAttribute()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
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
            
            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }
    }

    public class WhereAttributeIntEqualShould : SpanQueryServiceTests
    {
        [Fact]
        public async Task ReturnSpanWithMatchingAttribute()
        {
            var request = TestHelpers.CreateExportTraceServiceRequest();
            var intAttribute = TestHelpers.CreateKeyValue("test.int", 123);
            request.ResourceSpans[0].ScopeSpans[0].Spans[0].Attributes.Add(intAttribute);
            await _traceServiceClient.ExportAsync(request);
            
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
            
            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].SpanId);
        }
    }

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();
        _app = await builder.BuildAsync();
            
        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();

        await resourceNotificationService.WaitForResourceAsync("odd").WaitAsync(TimeSpan.FromSeconds(30));

        var endpoint = _app.GetEndpoint("odd", "http");
        var traceServiceChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _traceServiceClient = new TraceService.TraceServiceClient(traceServiceChannel);
            
        var spanQueryChannel = GrpcChannel.ForAddress(endpoint.AbsoluteUri);
        _spanQueryServiceClient = new SpanQueryService.SpanQueryServiceClient(spanQueryChannel);
    }

    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}