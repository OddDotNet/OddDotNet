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
            // Remove any channels from the list that have been completed
            Channels.RemoveAll(x => x.Reader.Completion.IsCompleted);
            
            // Write signal to remaining channels
            Channels.ForEach(x => x.Writer.TryWrite(signal));
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

    public bool RemoveChannel(Channel<Span> channel)
    {
        lock (Lock)
        {
            return Channels.Remove(channel);
        }
    }
}