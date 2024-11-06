using System.Threading.Channels;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet;

public class ChannelManager<TSignal> where TSignal : class, ISignal
{
    private static readonly List<Channel<TSignal>> Channels = [];
    private static readonly object Lock = new();
    
    public void NotifyChannels(TSignal signal)
    {
        lock (Lock)
        {
            Channels.ForEach(x => x.Writer.TryWrite(signal));
        }
    }
    
    public Channel<TSignal> AddChannel()
    {
        Channel<TSignal> channel = Channel.CreateUnbounded<TSignal>(
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
    
    public bool RemoveChannel(Channel<TSignal> channel)
    {
        lock (Lock)
        {
            return Channels.Remove(channel);
        }
    }
}