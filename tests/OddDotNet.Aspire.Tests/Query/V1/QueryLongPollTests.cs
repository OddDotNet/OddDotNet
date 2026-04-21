using System.Diagnostics;
using System.Text;
using System.Text.Json;

using Google.Protobuf;

using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;

using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OtelResourceSpans = OpenTelemetry.Proto.Trace.V1.ResourceSpans;
using OtelScopeSpans = OpenTelemetry.Proto.Trace.V1.ScopeSpans;

using Common = OddDotNet.Proto.Common.V1;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryLongPollTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public QueryLongPollTests(AspireFixture fixture) { _fixture = fixture; }

    [Fact]
    public async Task LongPoll_WhenMatchArrivesBeforeDeadline_ShouldReturnEarly()
    {
        await _fixture.HttpClient.DeleteAsync("/query/v1/spans");

        var name = $"longpoll-hit-{Guid.NewGuid():N}";
        var queryReq = new OddDotNet.Proto.Trace.V1.SpanQueryRequest
        {
            Take = new Common.Take { TakeFirst = new Common.TakeFirst() },
            Duration = new Common.Duration { Milliseconds = 5000 },
            Filters =
            {
                new OddDotNet.Proto.Trace.V1.Where
                {
                    Property = new OddDotNet.Proto.Trace.V1.PropertyFilter
                    {
                        Name = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = name }
                    }
                }
            }
        };

        var sw = Stopwatch.StartNew();
        var queryTask = QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/spans", queryReq);

        // Fire the producer after a small delay, on a separate task.
        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            await _fixture.TraceServiceClient.ExportAsync(new ExportTraceServiceRequest
            {
                ResourceSpans =
                {
                    new OtelResourceSpans
                    {
                        Resource = new Resource(),
                        ScopeSpans =
                        {
                            new OtelScopeSpans
                            {
                                Scope = new InstrumentationScope(),
                                Spans =
                                {
                                    new OtelSpan { Name = name, TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()), SpanId = ByteString.CopyFrom(new byte[8]{1,2,3,4,5,6,7,8}) }
                                }
                            }
                        }
                    }
                }
            });
        });

        var root = await queryTask;
        sw.Stop();

        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.True(sw.ElapsedMilliseconds < 4000, $"Should return well before 5s deadline, took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task LongPoll_WhenNoMatchBeforeDeadline_ShouldTimeOutWithEmptyItems()
    {
        var neverMatches = "never-matches-" + Guid.NewGuid().ToString("N");
        var req = new OddDotNet.Proto.Trace.V1.SpanQueryRequest
        {
            Take = new Common.Take { TakeFirst = new Common.TakeFirst() },
            Duration = new Common.Duration { Milliseconds = 400 },
            Filters =
            {
                new OddDotNet.Proto.Trace.V1.Where
                {
                    Property = new OddDotNet.Proto.Trace.V1.PropertyFilter
                    {
                        Name = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = neverMatches }
                    }
                }
            }
        };
        using var content = new StringContent(JsonFormatter.Default.Format(req), Encoding.UTF8, "application/json");

        var sw = Stopwatch.StartNew();
        var response = await _fixture.HttpClient.PostAsync("/query/v1/spans", content);
        sw.Stop();

        response.EnsureSuccessStatusCode();
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(0, root.GetProperty("count").GetInt32());
        Assert.True(sw.ElapsedMilliseconds >= 300, $"Should respect duration window, took {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 3000, $"Should not wait too long, took {sw.ElapsedMilliseconds}ms");
    }
}
