using System.Threading.Channels;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet;

public class AsyncTracesTelemetryList : IAsyncTelemetryList<Span>
{
    private static readonly Channel<Span> _channel = Channel.CreateUnbounded<Span>();
    
    public async Task<bool> AnyAsync(FilterDelegate<Span> filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<Span> FirstAsync(FilterDelegate<Span> filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool Add(Span element)
    {
        return _channel.Writer.TryWrite(element);
    }
}