using System.Diagnostics.CodeAnalysis;

namespace OddDotNet.Services.Query;

public class QueryDispatcher
{
    private readonly Dictionary<string, ISignalQueryHandler> _handlers;

    public QueryDispatcher(IEnumerable<ISignalQueryHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.SignalPath, StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGet(string signalPath, [NotNullWhen(true)] out ISignalQueryHandler? handler) =>
        _handlers.TryGetValue(signalPath, out handler);

    public void ResetAll()
    {
        foreach (var handler in _handlers.Values)
        {
            handler.Reset();
        }
    }

    public IReadOnlyCollection<string> KnownPaths => _handlers.Keys;
}
