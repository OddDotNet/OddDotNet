using System.Threading.Channels;
using OddDotNet.Proto.Metrics.V1;
using OddDotNet.Proto.Trace.V1;

namespace OddDotNet;

// public class ChannelManager
// {
//     private static readonly List<Channel<FlatSpan>> SpanChannels = [];
//     private static readonly List<Channel<FlatMetric>> MetricChannels = [];
//     
//     private static readonly object SpanLock = new();
//     private static readonly object MetricLock = new();
//     public void NotifyChannels(FlatSpan signal)
//     {
//         lock (SpanLock)
//         {
//             SpanChannels.ForEach(x => x.Writer.TryWrite(signal));
//         }
//     }
//
//     public void NotifyChannels(FlatMetric signal)
//     {
//         lock (MetricLock)
//         {
//             MetricChannels.ForEach(x => x.Writer.TryWrite(signal));
//         }
//     }
//
//     public Channel<FlatSpan> AddSpanChannel()
//     {
//         Channel<FlatSpan> channel = Channel.CreateUnbounded<FlatSpan>(
//             new UnboundedChannelOptions()
//             {
//                 SingleReader = true,
//                 SingleWriter = false
//             });
//         lock (SpanLock)
//         {
//             SpanChannels.Add(channel);
//         }
//
//         return channel;
//     }
//     
//     public Channel<FlatMetric> AddCMetricChannel()
//     {
//         Channel<FlatMetric> channel = Channel.CreateUnbounded<FlatMetric>(
//             new UnboundedChannelOptions()
//             {
//                 SingleReader = true,
//                 SingleWriter = false
//             });
//         lock (MetricLock)
//         {
//             MetricChannels.Add(channel);
//         }
//     
//         return channel;
//     }
//
//     public bool RemoveChannel(Channel<FlatSpan> channel)
//     {
//         lock (SpanLock)
//         {
//             return SpanChannels.Remove(channel);
//         }
//     }
//     
//     public bool RemoveChannel(Channel<FlatMetric> channel)
//     {
//         lock (SpanLock)
//         {
//             return MetricChannels.Remove(channel);
//         }
//     }
// }

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