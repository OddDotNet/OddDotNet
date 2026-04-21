using System.Text;

using Google.Protobuf;

using Microsoft.AspNetCore.Http;

using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.Query.Shorthand;

namespace OddDotNet.Services.Query;

public class SignalQueryHandler<TRequest, TFlat> : ISignalQueryHandler
    where TRequest : class, IMessage<TRequest>, new()
    where TFlat : class, ISignal, IMessage<TFlat>
{
    private readonly SignalList<TFlat> _signals;
    private readonly Func<TRequest, Take?> _getTake;
    private readonly Func<TRequest, Duration?> _getDuration;
    private readonly Func<TRequest, IReadOnlyCollection<IWhere<TFlat>>> _getFilters;
    private readonly IShorthandFilterBuilder<TRequest>? _shorthand;

    public string SignalPath { get; }

    public bool SupportsGetShorthand => _shorthand is not null;

    public SignalQueryHandler(
        string signalPath,
        SignalList<TFlat> signals,
        Func<TRequest, Take?> getTake,
        Func<TRequest, Duration?> getDuration,
        Func<TRequest, IReadOnlyCollection<IWhere<TFlat>>> getFilters,
        IShorthandFilterBuilder<TRequest>? shorthand = null)
    {
        SignalPath = signalPath;
        _signals = signals;
        _getTake = getTake;
        _getDuration = getDuration;
        _getFilters = getFilters;
        _shorthand = shorthand;
    }

    public Task<string> QueryAsJsonAsync(string jsonBody, CancellationToken cancellationToken)
    {
        TRequest request = string.IsNullOrWhiteSpace(jsonBody)
            ? new TRequest()
            : JsonParser.Default.Parse<TRequest>(jsonBody);

        return ExecuteAsync(request, cancellationToken);
    }

    public Task<string> QueryAsJsonFromQueryStringAsync(IQueryCollection query, CancellationToken cancellationToken)
    {
        if (_shorthand is null)
        {
            throw new NotSupportedException($"Signal '{SignalPath}' does not support GET shorthand.");
        }

        TRequest request = _shorthand.Build(query);
        return ExecuteAsync(request, cancellationToken);
    }

    public void Reset() => _signals.Reset();

    private async Task<string> ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        var take = _getTake(request);
        var duration = _getDuration(request);
        var filters = _getFilters(request);

        int takeCap = ResolveTakeCap(take);

        var itemJsons = new List<string>();
        await foreach (var signal in _signals.QueryAsync(take, duration, filters)
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            itemJsons.Add(JsonFormatter.Default.Format(signal));
        }

        bool truncated = takeCap != int.MaxValue && itemJsons.Count == takeCap;

        return BuildResponseJson(itemJsons, truncated);
    }

    private static int ResolveTakeCap(Take? take)
    {
        var t = take ?? new Take { TakeFirst = new TakeFirst() };
        return t.ValueCase switch
        {
            Take.ValueOneofCase.TakeFirst => 1,
            Take.ValueOneofCase.TakeAll => int.MaxValue,
            Take.ValueOneofCase.TakeExact => t.TakeExact.Count,
            _ => 0
        };
    }

    private static string BuildResponseJson(List<string> itemJsons, bool truncated)
    {
        var sb = new StringBuilder();
        sb.Append("{\"items\":[");
        for (int i = 0; i < itemJsons.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(itemJsons[i]);
        }
        sb.Append("],\"count\":").Append(itemJsons.Count);
        sb.Append(",\"truncated\":").Append(truncated ? "true" : "false");
        sb.Append('}');
        return sb.ToString();
    }
}
