namespace OddDotNet;

public interface IAsyncTelemetryList <TElement>
    where TElement : class
{
    Task<bool> AnyAsync(FilterDelegate<TElement> filter, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    Task<TElement> FirstAsync(FilterDelegate<TElement> filter, CancellationToken cancellationToken = default);
    bool Add(TElement element);
}