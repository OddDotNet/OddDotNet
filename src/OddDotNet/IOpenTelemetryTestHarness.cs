using OpenTelemetry.Proto.Logs.V1;
using OpenTelemetry.Proto.Metrics.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet;

public interface IOpenTelemetryTestHarness
{
    IAsyncTelemetryList<Span> Traces { get; }
    IAsyncTelemetryList<LogRecord> Logs { get; }
    IAsyncTelemetryList<Metric> Metrics { get; }

    // Task StartAsync(CancellationToken cancellationToken = default);
    // Task StopAsync(CancellationToken cancellationToken = default);
}