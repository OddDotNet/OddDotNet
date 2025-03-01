using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using OddDotNet.Proto.Common.V1;

namespace OddDotNet;

public class SignalList<TSignal> : ISignalList where TSignal : class, ISignal
{
    private readonly ChannelManager<TSignal> _channels;
    private readonly TimeProvider _timeProvider;
    private readonly OddSettings _oddSettings;
    private readonly ILogger<SignalList<TSignal>> _logger;

    private static readonly Queue<Expirable<TSignal>> Signals = [];
    
    private static readonly object Lock = new();
    
    public SignalList(ChannelManager<TSignal> channels, TimeProvider timeProvider, ILogger<SignalList<TSignal>> logger,
        IOptions<OddSettings> oddSettings)
    {
        _channels = channels;
        _timeProvider = timeProvider;
        _logger = logger;
        _oddSettings = oddSettings.Value;
    }

    public void Add(TSignal signal)
    {
        lock (Lock)
        {
            DateTimeOffset expiresAt = _timeProvider.GetUtcNow().AddMilliseconds(_oddSettings.Cache.Expiration);
            Signals.Enqueue(new Expirable<TSignal>(signal, expiresAt));
            
            _channels.NotifyChannels(signal);
        }
    }

    public async IAsyncEnumerable<TSignal> QueryAsync(Take? take, Duration? duration,
        IReadOnlyCollection<IWhere<TSignal>> filters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var timeout = GetQueryTimeout(duration?.Milliseconds ?? -1);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

        Channel<TSignal> channel = _channels.AddChannel();

        try
        {
            lock (Lock)
            {
                foreach (var expirable in Signals)
                    channel.Writer.TryWrite(expirable.Signal);
            }

            int takeCount = GetTakeCount(take ?? new Take { TakeFirst = new() });
            int currentCount = 0;

            while (currentCount < takeCount && !cts.IsCancellationRequested)
            {
                TSignal? signal;
                try
                {
                    await channel.Reader.WaitToReadAsync(cts.Token);
                    signal = await channel.Reader.ReadAsync(cts.Token);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogDebug(ex, "The query operation was cancelled");
                    break;
                }

                if (ShouldInclude(filters, signal))
                {
                    yield return signal;
                    currentCount++;
                }
            }
        }
        finally
        {
            _channels.RemoveChannel(channel);
            channel.Writer.TryComplete();
        }
    }
    
    public void Reset()
    {
        lock (Lock)
        {
            Signals.Clear();
        }
    }
    
    public void Prune()
    {
        _logger.LogDebug("Prune");
        lock (Lock)
        {
            DateTimeOffset currentTime = _timeProvider.GetUtcNow();
            int numRemoved = 0;
            while (Signals.TryPeek(out var result) && result.ExpireAt < currentTime)
            {
                Signals.Dequeue();
                numRemoved++;
            }
            _logger.LogDebug("Removed {numRemoved} signals", numRemoved);
        }
    }
    
    private static int GetTakeCount(Take take) => take.ValueCase switch
    {
         Take.ValueOneofCase.TakeFirst => 1,
         Take.ValueOneofCase.TakeAll => int.MaxValue,
         Take.ValueOneofCase.TakeExact => take.TakeExact.Count,
         Take.ValueOneofCase.None => 0,
         _ => 0
    };
    
    private static CancellationTokenSource GetQueryTimeout(int duration) =>
        duration <= 0
         ? new CancellationTokenSource(int.MaxValue)
         : new CancellationTokenSource(TimeSpan.FromMilliseconds(duration));

    private static bool ShouldInclude(IReadOnlyCollection<IWhere<TSignal>> filters, TSignal signal) =>
        filters.Count == 0 || filters.All(filter => filter.Matches(signal));
}