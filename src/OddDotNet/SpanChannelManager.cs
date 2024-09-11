using System.Collections.Concurrent;
using System.Threading.Channels;

namespace OddDotNet;

public class SpanChannelManager : IChannelManager<Span>
{
    private static readonly List<Channel<Span>> Channels = [];
    
    private static readonly object Lock = new();
    public void NotifyChannels(Span signal)
    {
        lock (Lock)
        {
            foreach (Channel<Span> channel in Channels)
            {
                channel.Writer.TryWrite(signal);
            }
        }
    }

    public Channel<Span> AddChannel()
    {
        Channel<Span> channel = Channel.CreateUnbounded<Span>(
            new UnboundedChannelOptions()
            {
                SingleReader = true,
                SingleWriter = false
            });
        lock (Lock)
        {
            Channels.Add(channel);
        }

        return channel;
    }
}