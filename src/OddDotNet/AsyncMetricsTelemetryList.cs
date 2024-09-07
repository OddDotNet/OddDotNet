using System.Diagnostics.Metrics;
using System.Threading.Channels;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet;

public class AsyncMetricsTelemetryList : IAsyncTelemetryList<Metric>
{
    private static readonly Channel<Metric> _channel = Channel.CreateUnbounded<Metric>();

    public async Task<bool> AnyAsync(FilterDelegate<Metric> filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await _channel.Reader.WaitToReadAsync(cancellationToken);
    }

    public Task<Metric> FirstAsync(FilterDelegate<Metric> filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool Add(Metric element)
    {
        return _channel.Writer.TryWrite(element);
    }
}