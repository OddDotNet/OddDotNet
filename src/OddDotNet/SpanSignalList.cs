using System.Threading.Channels;
using Google.Protobuf;

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
            PruneExpiredSpans();
            
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
        cts.CancelAfter(TimeSpan.FromSeconds(300));
        
        // Create the channel and populate it with the current contents of the span list
        Channel<Span> channel = _channels.AddChannel();
        lock (Lock)
        {
            PruneExpiredSpans();
            
            foreach (var expirableSpan in Spans)
            {
                channel.Writer.TryWrite(expirableSpan.Signal);
            }
        }
        
        SpanQueryRequest spanRequest = request as SpanQueryRequest ?? throw new InvalidCastException(nameof(request));
        List<Span> matchingSpans = [];
        int takeCount = GetTakeCount(spanRequest);

        while (matchingSpans.Count < takeCount && !cts.IsCancellationRequested)
        {
            await channel.Reader.WaitToReadAsync(cts.Token);
            Span span = await channel.Reader.ReadAsync(cts.Token);
            
            if (ShouldInclude(spanRequest, span))
                matchingSpans.Add(span);
        }

        return matchingSpans;
    }

    private void PruneExpiredSpans()
    {
        DateTimeOffset currentTime = _timeProvider.GetUtcNow();
        Spans.RemoveAll(expirable => expirable.ExpireAt < currentTime);
    }

    private int GetTakeCount(SpanQueryRequest spanQueryRequest) => spanQueryRequest.Take.TakeTypeCase switch
    {
        Take.TakeTypeOneofCase.TakeFirst => 1,
        Take.TakeTypeOneofCase.TakeAll => int.MaxValue,
        Take.TakeTypeOneofCase.TakeExact => spanQueryRequest.Take.TakeExact.Count,
        Take.TakeTypeOneofCase.None => throw new Exception("Take type invalid"), // TODO change to better exception,
        _ => throw new Exception("Take type invalid") // TODO change to better exception
    };

    private static bool ShouldInclude(SpanQueryRequest spanQueryRequest, Span span)
    {
        foreach (Where whereFilter in spanQueryRequest.WhereFilters)
        {
            if (!SpanMatchesWhereFilter(whereFilter, span))
                return false;
        }

        return true;
    }

    private static bool SpanMatchesWhereFilter(Where filter, Span span) => filter.FilterCase switch
    {
        Where.FilterOneofCase.AttributeStringEqual => ProcessWhereAttributeStringEqualFilter(
            filter.AttributeStringEqual, span),
        Where.FilterOneofCase.AttributeExists => ProcessWhereAttributeExistsFilter(filter.AttributeExists, span),
        _ => throw new NotImplementedException("Something went wrong"),
    };

    private static bool ProcessWhereAttributeStringEqualFilter(WhereAttributeStringEqualFilter filter, Span span)
    {
        bool matched = false;
        
        if (span.Attributes.TryGetValue(filter.Attribute, out var attribute))
        {
            var blah = attribute.Value.ToString();
            
            string value = attribute.Value.ToStringUtf8();
            
            matched = string.Equals(value, filter.Compare, StringComparison.Ordinal);
        }
        
        return matched;
    }

    private static bool ProcessWhereAttributeExistsFilter(WhereAttributeExistsFilter filter, Span span)
    {
        return span.Attributes.ContainsKey(filter.Attribute);
    }
}