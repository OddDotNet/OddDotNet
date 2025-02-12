using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Proto.Trace.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;
using SpanKindProperty = OddDotNet.Proto.Trace.V1.SpanKindProperty;
using SpanStatusCodeProperty = OddDotNet.Proto.Trace.V1.SpanStatusCodeProperty;

namespace OddDotNet.Aspire.Tests.Trace.V1;

public class SpanQueryServiceTests : IAsyncLifetime
{
    #pragma warning disable CS8618
    private TraceService.TraceServiceClient _traceServiceClient;
    private SpanQueryService.SpanQueryServiceClient _spanQueryServiceClient;
    private DistributedApplication _app;
    #pragma warning disable CS8618

    public class WhereSpanPropertyShould : SpanQueryServiceTests
    {
        [Fact]
        public async Task ReturnSpansWithMatchingStatusCodeProperty()
        {
            var request = TraceHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var codeToFind = spanToFind.Status.Code;

            var take = new Take
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new Where
            {
                Property = new PropertyFilter
                {
                    Status = new StatusFilter
                    {
                        Code = new SpanStatusCodeProperty
                        {
                            CompareAs = EnumCompareAsType.Equals,
                            Compare = codeToFind
                        }
                    }
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter } };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].Span.SpanId);
        }
        
        [Fact]
        public async Task ReturnSpansWithMatchingStatusMessageProperty()
        {
            var request = TraceHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var messageToFind = spanToFind.Status.Message;

            var take = new Take
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new Where
            {
                Property = new PropertyFilter
                {
                    Status = new StatusFilter
                    {
                        Message = new StringProperty
                        {
                            CompareAs = StringCompareAsType.Equals,
                            Compare = messageToFind
                        }
                    }
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter } };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].Span.SpanId);
        }
        
        [Fact]
        public async Task ReturnSpansWithMatchingKindProperty()
        {
            var request = TraceHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request);
            
            var spanToFind = request.ResourceSpans[0].ScopeSpans[0].Spans[0];
            var codeToFind = spanToFind.Kind;

            var take = new Take()
            {
                TakeFirst = new TakeFirst()
            };

            var whereFilter = new Where()
            {
                Property = new PropertyFilter()
                {
                    Kind = new SpanKindProperty()
                    {
                        CompareAs = EnumCompareAsType.Equals,
                        Compare = codeToFind
                    }
                }
            };
            
            var spanQueryRequest = new SpanQueryRequest() { Take = take, Filters = { whereFilter } };

            var response = await _spanQueryServiceClient.QueryAsync(spanQueryRequest);
            
            Assert.NotEmpty(response.Spans);
            Assert.Equal(spanToFind.SpanId, response.Spans[0].Span.SpanId);
        }

        [Fact]
        public async Task ReturnSpansAsStream()
        {
            var request1 = TraceHelpers.CreateExportTraceServiceRequest();
            var request2 = TraceHelpers.CreateExportTraceServiceRequest();
            await _traceServiceClient.ExportAsync(request1);
            await _traceServiceClient.ExportAsync(request2);

            var queryRequest = new SpanQueryRequest
            {
                Take = new Take
                {
                    TakeAll = new TakeAll()
                },
                Duration = new Duration
                {
                    Milliseconds = 1000
                },
                Filters =
                {
                    new Where
                    {
                        Or = new OrFilter
                        {
                            Filters =
                            {
                                new Where
                                {
                                    Property = new PropertyFilter
                                    {
                                        SpanId = new ByteStringProperty
                                        {
                                            CompareAs = ByteStringCompareAsType.Equals,
                                            Compare = request1.ResourceSpans[0].ScopeSpans[0].Spans[0].SpanId
                                        }
                                    }
                                },
                                new Where
                                {
                                    Property = new PropertyFilter
                                    {
                                        SpanId = new ByteStringProperty
                                        {
                                            CompareAs = ByteStringCompareAsType.Equals,
                                            Compare = request2.ResourceSpans[0].ScopeSpans[0].Spans[0].SpanId
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            
            List<FlatSpan> spans = new List<FlatSpan>();
            await foreach (FlatSpan span in _spanQueryServiceClient.StreamQuery(queryRequest).ResponseStream.ReadAllAsync())
                spans.Add(span);
            
            Assert.Equal(2, spans.Count);
        }
    }

    /// <summary>
    /// Builds and starts the AppHost project, which has a single OddDotNet project defined within.
    /// Once started, configures the clients for exporting and querying.
    /// </summary>
    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();
        _app = await builder.BuildAsync();
            
        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();

        await resourceNotificationService.WaitForResourceAsync("odd").WaitAsync(TimeSpan.FromSeconds(30));

        var endpoint = _app.GetEndpoint("odd", "grpc");
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