namespace OddDotNet;

public class SpanSignalList : ISignalList<Span>
{
    private readonly IChannelManager<Span> _channels;

    public SpanSignalList(IChannelManager<Span> channels)
    {
        _channels = channels;
    }

    public void Add(Span signal)
    {
        // TODO Add span to list
        
        // Notify any listening channels
        _channels.Notify(signal);
        
        throw new NotImplementedException();
    }

    public List<Span> Query(IQueryRequest<Span> request)
    {
        SpanQueryRequest spanRequest = request as SpanQueryRequest ?? throw new InvalidCastException(nameof(request));
        
        // TODO 
        throw new NotImplementedException();
    }
}