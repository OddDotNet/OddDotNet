using System.Threading.Channels;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet;

public class SpanChannelManager : IChannelManager<FlatSpan>
{
    private static readonly List<Channel<FlatSpan>> Channels = [];
    
    private static readonly object Lock = new();
    public void NotifyChannels(FlatSpan signal)
    {
        lock (Lock)
        {
            // Remove any channels from the list that have been completed
            Channels.RemoveAll(x => x.Reader.Completion.IsCompleted);
            
            // Write signal to remaining channels
            Channels.ForEach(x => x.Writer.TryWrite(signal));
        }
    }

    public Channel<FlatSpan> AddChannel()
    {
        Channel<FlatSpan> channel = Channel.CreateUnbounded<FlatSpan>(
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

    public bool RemoveChannel(Channel<FlatSpan> channel)
    {
        lock (Lock)
        {
            return Channels.Remove(channel);
        }
    }
}