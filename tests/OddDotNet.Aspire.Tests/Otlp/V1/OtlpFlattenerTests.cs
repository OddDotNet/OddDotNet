using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using OddDotNet.Proto.Logs.V1;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Trace.V1;
using OddDotNet.Services.Otlp;

using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Logs.V1;
using OpenTelemetry.Proto.Metrics.V1;
using OpenTelemetry.Proto.Resource.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests.Otlp.V1;

public class OtlpFlattenerTests
{
    private static SignalList<T> CreateSignalList<T>() where T : class, ISignal
    {
        var options = Options.Create(new OddSettings());
        return new SignalList<T>(
            new ChannelManager<T>(),
            TimeProvider.System,
            NullLogger<SignalList<T>>.Instance,
            options);
    }

    [Fact]
    public void FlattenTraces_WhenRequestEmpty_ShouldAddNoSignals()
    {
        var signals = CreateSignalList<FlatSpan>();
        signals.Reset();

        OtlpFlattener.Flatten(new ExportTraceServiceRequest(), signals);

        Assert.Equal(0, signals.Count);
    }

    [Fact]
    public void FlattenTraces_WhenSingleSpan_ShouldPreserveResourceScopeAndSchemaUrls()
    {
        var signals = CreateSignalList<FlatSpan>();
        signals.Reset();

        var resource = new Resource { Attributes = { new KeyValue { Key = "service.name", Value = new AnyValue { StringValue = "svc-a" } } } };
        var scope = new InstrumentationScope { Name = "scope-a", Version = "1.0.0" };
        var span = new Span { Name = "op-a", TraceId = Google.Protobuf.ByteString.CopyFromUtf8("trace-1234567890123456"), SpanId = Google.Protobuf.ByteString.CopyFromUtf8("span-123") };

        var request = new ExportTraceServiceRequest
        {
            ResourceSpans =
            {
                new ResourceSpans
                {
                    Resource = resource,
                    SchemaUrl = "https://schemas/resource",
                    ScopeSpans =
                    {
                        new ScopeSpans
                        {
                            Scope = scope,
                            SchemaUrl = "https://schemas/scope",
                            Spans = { span }
                        }
                    }
                }
            }
        };

        OtlpFlattener.Flatten(request, signals);

        Assert.Equal(1, signals.Count);
        var flat = signals.GetAll()[0];
        Assert.Equal(span, flat.Span);
        Assert.Equal(scope, flat.InstrumentationScope);
        Assert.Equal(resource, flat.Resource);
        Assert.Equal("https://schemas/resource", flat.ResourceSchemaUrl);
        Assert.Equal("https://schemas/scope", flat.InstrumentationScopeSchemaUrl);
    }

    [Fact]
    public void FlattenTraces_WhenMultipleResourcesScopesAndSpans_ShouldProduceCartesianCount()
    {
        var signals = CreateSignalList<FlatSpan>();
        signals.Reset();

        var request = new ExportTraceServiceRequest();
        // 2 resources x 2 scopes x 3 spans = 12
        for (int r = 0; r < 2; r++)
        {
            var rs = new ResourceSpans { Resource = new Resource(), SchemaUrl = $"r{r}" };
            for (int s = 0; s < 2; s++)
            {
                var ss = new ScopeSpans { Scope = new InstrumentationScope { Name = $"s{r}{s}" }, SchemaUrl = $"s{r}{s}" };
                for (int sp = 0; sp < 3; sp++)
                {
                    ss.Spans.Add(new Span { Name = $"op{r}{s}{sp}" });
                }
                rs.ScopeSpans.Add(ss);
            }
            request.ResourceSpans.Add(rs);
        }

        OtlpFlattener.Flatten(request, signals);

        Assert.Equal(12, signals.Count);
    }

    [Fact]
    public void FlattenMetrics_WhenRequestEmpty_ShouldAddNoSignals()
    {
        var signals = CreateSignalList<FlatMetric>();
        signals.Reset();

        OtlpFlattener.Flatten(new ExportMetricsServiceRequest(), signals);

        Assert.Equal(0, signals.Count);
    }

    [Fact]
    public void FlattenMetrics_WhenSingleMetric_ShouldPreserveResourceScopeAndSchemaUrls()
    {
        var signals = CreateSignalList<FlatMetric>();
        signals.Reset();

        var resource = new Resource();
        var scope = new InstrumentationScope { Name = "scope-m" };
        var metric = new Metric { Name = "m1" };

        var request = new ExportMetricsServiceRequest
        {
            ResourceMetrics =
            {
                new ResourceMetrics
                {
                    Resource = resource,
                    SchemaUrl = "r-schema",
                    ScopeMetrics =
                    {
                        new ScopeMetrics
                        {
                            Scope = scope,
                            SchemaUrl = "s-schema",
                            Metrics = { metric }
                        }
                    }
                }
            }
        };

        OtlpFlattener.Flatten(request, signals);

        Assert.Equal(1, signals.Count);
        var flat = signals.GetAll()[0];
        Assert.Equal(metric, flat.Metric);
        Assert.Equal(scope, flat.InstrumentationScope);
        Assert.Equal(resource, flat.Resource);
        Assert.Equal("r-schema", flat.ResourceSchemaUrl);
        Assert.Equal("s-schema", flat.InstrumentationScopeSchemaUrl);
    }

    [Fact]
    public void FlattenMetrics_WhenMultipleResourcesScopesAndMetrics_ShouldProduceCartesianCount()
    {
        var signals = CreateSignalList<FlatMetric>();
        signals.Reset();

        var request = new ExportMetricsServiceRequest();
        for (int r = 0; r < 2; r++)
        {
            var rm = new ResourceMetrics { Resource = new Resource() };
            for (int s = 0; s < 2; s++)
            {
                var sm = new ScopeMetrics { Scope = new InstrumentationScope { Name = $"s{r}{s}" } };
                for (int m = 0; m < 3; m++)
                {
                    sm.Metrics.Add(new Metric { Name = $"m{r}{s}{m}" });
                }
                rm.ScopeMetrics.Add(sm);
            }
            request.ResourceMetrics.Add(rm);
        }

        OtlpFlattener.Flatten(request, signals);

        Assert.Equal(12, signals.Count);
    }

    [Fact]
    public void FlattenLogs_WhenRequestEmpty_ShouldAddNoSignals()
    {
        var signals = CreateSignalList<FlatLog>();
        signals.Reset();

        OtlpFlattener.Flatten(new ExportLogsServiceRequest(), signals);

        Assert.Equal(0, signals.Count);
    }

    [Fact]
    public void FlattenLogs_WhenSingleLog_ShouldPreserveResourceScopeAndSchemaUrls()
    {
        var signals = CreateSignalList<FlatLog>();
        signals.Reset();

        var resource = new Resource();
        var scope = new InstrumentationScope { Name = "scope-l" };
        var log = new LogRecord { Body = new AnyValue { StringValue = "hello" } };

        var request = new ExportLogsServiceRequest
        {
            ResourceLogs =
            {
                new ResourceLogs
                {
                    Resource = resource,
                    SchemaUrl = "r-schema",
                    ScopeLogs =
                    {
                        new ScopeLogs
                        {
                            Scope = scope,
                            SchemaUrl = "s-schema",
                            LogRecords = { log }
                        }
                    }
                }
            }
        };

        OtlpFlattener.Flatten(request, signals);

        Assert.Equal(1, signals.Count);
        var flat = signals.GetAll()[0];
        Assert.Equal(log, flat.Log);
        Assert.Equal(scope, flat.InstrumentationScope);
        Assert.Equal(resource, flat.Resource);
        Assert.Equal("r-schema", flat.ResourceSchemaUrl);
        Assert.Equal("s-schema", flat.InstrumentationScopeSchemaUrl);
    }

    [Fact]
    public void FlattenLogs_WhenMultipleResourcesScopesAndLogs_ShouldProduceCartesianCount()
    {
        var signals = CreateSignalList<FlatLog>();
        signals.Reset();

        var request = new ExportLogsServiceRequest();
        for (int r = 0; r < 2; r++)
        {
            var rl = new ResourceLogs { Resource = new Resource() };
            for (int s = 0; s < 2; s++)
            {
                var sl = new ScopeLogs { Scope = new InstrumentationScope { Name = $"s{r}{s}" } };
                for (int lg = 0; lg < 3; lg++)
                {
                    sl.LogRecords.Add(new LogRecord { Body = new AnyValue { StringValue = $"l{r}{s}{lg}" } });
                }
                rl.ScopeLogs.Add(sl);
            }
            request.ResourceLogs.Add(rl);
        }

        OtlpFlattener.Flatten(request, signals);

        Assert.Equal(12, signals.Count);
    }
}
