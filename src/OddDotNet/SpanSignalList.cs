using System.Threading.Channels;

namespace OddDotNet;

public class SpanSignalList : ISignalList<Span>
{
    private readonly IChannelManager<Span> _channels;
    private readonly TimeProvider _timeProvider;

    private static readonly List<Expirable<Span>> Spans = [];

    private static readonly object Lock = new();
    public SpanSignalList(IChannelManager<Span> channels, TimeProvider timeProvider)
    {
        _channels = channels;
        _timeProvider = timeProvider;
    }

    public void Add(Span signal)
    {
        lock (Lock)
        {
            DateTimeOffset currentTime = _timeProvider.GetUtcNow();
            
            // Remove all expired spans before doing anything else.
            Spans.RemoveAll(expirable => expirable.ExpireAt < currentTime);
            
            // Add the new span with 30 second expire
            // TODO make this configurable
            DateTimeOffset expiresAt = _timeProvider.GetUtcNow().AddSeconds(30);
            Spans.Add(new Expirable<Span>(signal, expiresAt));
            
            // Notify any listening channels
            _channels.NotifyChannels(signal);
        }
    }

    public async Task<List<Span>> QueryAsync(IQueryRequest<Span> request, CancellationToken cancellationToken = default)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        // TODO Make this configurable
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        
        // Create the channel and populate it with the current contents of the span list
        Channel<Span> channel = _channels.AddChannel();
        lock (Lock)
        {
            foreach (var expirableSpan in Spans)
            {
                channel.Writer.TryWrite(expirableSpan.Signal);
            }
        }
        
        SpanQueryRequest spanRequest = request as SpanQueryRequest ?? throw new InvalidCastException(nameof(request));
        List<Span> matchingSpans = [];

        while (!cts.IsCancellationRequested)
        {
            await channel.Reader.WaitToReadAsync(cts.Token);
            Span span = await channel.Reader.ReadAsync(cts.Token);
            
            // TODO We need to include a count property on the query as well. If you want the first instance, Count = 1.
            if (ShouldInclude(spanRequest, span))
                matchingSpans.Add(span);
        }

        return matchingSpans;
    }

    private static bool ShouldInclude(SpanQueryRequest spanQueryRequest, Span span)
    {
        foreach (Where whereFilter in spanQueryRequest.WhereFilters)
        {
            if (!ProcessWhereFilter(whereFilter, span))
                return false;
        }

        return true;
    }

    private static bool ProcessWhereFilter(Where filter, Span span) => filter.FilterCase switch
    {
        Where.FilterOneofCase.AttributeStringEqual => ProcessWhereAttributeStringEqualFilter(
            filter.AttributeStringEqual, span),
        _ => throw new NotImplementedException("Something went wrong"),
    };

    private static bool ProcessWhereAttributeStringEqualFilter(WhereAttributeStringEqualFilter filter, Span span)
    {
        bool matched = false;
        
        if (span.Attributes.TryGetValue(filter.Attribute, out var attribute))
        {
            string value = attribute as string ?? throw new InvalidCastException($"Attribute {filter.Attribute} is not a string");
            matched = value.Equals(filter.Compare);
        }
        
        return matched;
    }
}