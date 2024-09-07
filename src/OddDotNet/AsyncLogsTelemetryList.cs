using System.Threading.Channels;
using OpenTelemetry.Proto.Logs.V1;

namespace OddDotNet;

public class AsyncLogsTelemetryList : IAsyncTelemetryList<LogRecord>
{
    private static readonly Channel<LogRecord> _channel = Channel.CreateUnbounded<LogRecord>();

    public async Task<bool> AnyAsync(FilterDelegate<LogRecord> filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<LogRecord> FirstAsync(FilterDelegate<LogRecord> filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool Add(LogRecord element)
    {
        return _channel.Writer.TryWrite(element);
    }
}