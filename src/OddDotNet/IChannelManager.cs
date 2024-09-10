namespace OddDotNet;

public interface IChannelManager<in TSignal> where TSignal : class
{
    void Notify(TSignal signal);
}