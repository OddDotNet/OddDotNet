using System.Threading.Channels;

namespace OddDotNet;

public interface IChannelManager<TSignal> where TSignal : class
{
    void NotifyChannels(TSignal signal);
    Channel<TSignal> AddChannel();
    bool RemoveChannel(Channel<TSignal> channel);
}