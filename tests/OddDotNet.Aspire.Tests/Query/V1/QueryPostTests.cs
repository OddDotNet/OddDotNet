using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Google.Protobuf;

using OddDotNet.Services.AppInsights;

using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Resource.V1;

using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OtelResourceSpans = OpenTelemetry.Proto.Trace.V1.ResourceSpans;
using OtelScopeSpans = OpenTelemetry.Proto.Trace.V1.ScopeSpans;

using Common = OddDotNet.Proto.Common.V1;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryPostTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public QueryPostTests(AspireFixture fixture) { _fixture = fixture; }

    private static Common.Duration ShortDuration => new() { Milliseconds = 500 };
    private static Common.Take TakeAll => new() { TakeAll = new Common.TakeAll() };

    // ===== OTLP =====

    [Fact]
    public async Task PostSpans_HappyPath()
    {
        var name = $"q-span-{Guid.NewGuid():N}";
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

        var q = new OddDotNet.Proto.Trace.V1.SpanQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.Trace.V1.Where { Property = new OddDotNet.Proto.Trace.V1.PropertyFilter { Name = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = name } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/spans", q);
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostMetrics_HappyPath()
    {
        var req = MetricHelpers.CreateExportMetricsServiceRequest();
        var name = req.ResourceMetrics[0].ScopeMetrics[0].Metrics[0].Name;
        await _fixture.MetricsServiceClient.ExportAsync(req);

        var q = new OddDotNet.Proto.Metrics.V1.MetricQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.Metrics.V1.Where { Property = new OddDotNet.Proto.Metrics.V1.PropertyFilter { Name = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = name } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/metrics", q);
        Assert.True(root.GetProperty("count").GetInt32() >= 1);
    }

    [Fact]
    public async Task PostLogs_HappyPath()
    {
        var req = LogHelpers.CreateExportLogsServiceRequest();
        var traceId = req.ResourceLogs[0].ScopeLogs[0].LogRecords[0].TraceId;
        await _fixture.LogsServiceClient.ExportAsync(req);

        var q = new OddDotNet.Proto.Logs.V1.LogQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.Logs.V1.Where { Property = new OddDotNet.Proto.Logs.V1.PropertyFilter { TraceId = new Common.ByteStringProperty { CompareAs = Common.ByteStringCompareAsType.Equals, Compare = traceId } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/logs", q);
        Assert.True(root.GetProperty("count").GetInt32() >= 1);
    }

    // ===== App Insights =====

    private async Task IngestAppInsights(AppInsightsTelemetryEnvelope envelope)
    {
        using var content = new StringContent(JsonSerializer.Serialize(envelope), Encoding.UTF8, "application/json");
        await _fixture.HttpClient.PostAsync("/v2/track", content);
    }

    [Fact]
    public async Task PostAppInsightsRequests_HappyPath()
    {
        var env = AppInsightsHelpers.CreateRequestEnvelope();
        var uniqueId = $"q-req-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Id = uniqueId;
        await IngestAppInsights(env);

        var q = new OddDotNet.Proto.AppInsights.V1.Request.RequestQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.AppInsights.V1.Request.Where { Property = new OddDotNet.Proto.AppInsights.V1.Request.PropertyFilter { Id = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = uniqueId } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/appinsights/requests", q);
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostAppInsightsDependencies_HappyPath()
    {
        var env = AppInsightsHelpers.CreateDependencyEnvelope();
        var uniqueId = $"q-dep-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Id = uniqueId;
        await IngestAppInsights(env);

        var q = new OddDotNet.Proto.AppInsights.V1.Dependency.DependencyQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.AppInsights.V1.Dependency.Where { Property = new OddDotNet.Proto.AppInsights.V1.Dependency.PropertyFilter { Id = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = uniqueId } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/appinsights/dependencies", q);
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostAppInsightsExceptions_HappyPath()
    {
        var env = AppInsightsHelpers.CreateExceptionEnvelope();
        var uniqueId = $"q-exc-{Guid.NewGuid():N}";
        env.Data!.BaseData!.ProblemId = uniqueId;
        await IngestAppInsights(env);

        var q = new OddDotNet.Proto.AppInsights.V1.Exception.ExceptionQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.AppInsights.V1.Exception.Where { Property = new OddDotNet.Proto.AppInsights.V1.Exception.PropertyFilter { ProblemId = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = uniqueId } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/appinsights/exceptions", q);
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostAppInsightsTraces_HappyPath()
    {
        var env = AppInsightsHelpers.CreateTraceEnvelope();
        var uniqueMsg = $"q-trc-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Message = uniqueMsg;
        await IngestAppInsights(env);

        var q = new OddDotNet.Proto.AppInsights.V1.Trace.TraceQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.AppInsights.V1.Trace.Where { Property = new OddDotNet.Proto.AppInsights.V1.Trace.PropertyFilter { Message = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = uniqueMsg } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/appinsights/traces", q);
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostAppInsightsEvents_HappyPath()
    {
        var env = AppInsightsHelpers.CreateEventEnvelope();
        var uniqueName = $"q-evt-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Name = uniqueName;
        await IngestAppInsights(env);

        var q = new OddDotNet.Proto.AppInsights.V1.Event.EventQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.AppInsights.V1.Event.Where { Property = new OddDotNet.Proto.AppInsights.V1.Event.PropertyFilter { Name = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = uniqueName } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/appinsights/events", q);
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostAppInsightsMetrics_HappyPath()
    {
        var env = AppInsightsHelpers.CreateMetricEnvelope();
        var uniqueName = $"q-ametric-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Metrics![0].Name = uniqueName;
        await IngestAppInsights(env);

        var q = new OddDotNet.Proto.AppInsights.V1.Metric.MetricQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.AppInsights.V1.Metric.Where { Property = new OddDotNet.Proto.AppInsights.V1.Metric.PropertyFilter { Name = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = uniqueName } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/appinsights/metrics", q);
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostAppInsightsPageViews_HappyPath()
    {
        var env = AppInsightsHelpers.CreatePageViewEnvelope();
        var uniqueId = $"q-pv-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Id = uniqueId;
        await IngestAppInsights(env);

        var q = new OddDotNet.Proto.AppInsights.V1.PageView.PageViewQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.AppInsights.V1.PageView.Where { Property = new OddDotNet.Proto.AppInsights.V1.PageView.PropertyFilter { Id = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = uniqueId } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/appinsights/pageviews", q);
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostAppInsightsAvailability_HappyPath()
    {
        var env = AppInsightsHelpers.CreateAvailabilityEnvelope();
        var uniqueId = $"q-avail-{Guid.NewGuid():N}";
        env.Data!.BaseData!.Id = uniqueId;
        await IngestAppInsights(env);

        var q = new OddDotNet.Proto.AppInsights.V1.Availability.AvailabilityQueryRequest
        {
            Take = TakeAll, Duration = ShortDuration,
            Filters = { new OddDotNet.Proto.AppInsights.V1.Availability.Where { Property = new OddDotNet.Proto.AppInsights.V1.Availability.PropertyFilter { Id = new Common.StringProperty { CompareAs = Common.StringCompareAsType.Equals, Compare = uniqueId } } } }
        };

        var root = await QueryTestHelpers.PostQueryAsync(_fixture.HttpClient, "/query/v1/appinsights/availability", q);
        Assert.Equal(1, root.GetProperty("count").GetInt32());
    }
}
