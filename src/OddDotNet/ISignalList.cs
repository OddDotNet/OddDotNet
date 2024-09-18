using System.Runtime.CompilerServices;

namespace OddDotNet;

public interface ISignalList<TSignal> where TSignal : class
{
    void Add(TSignal signal);
    IAsyncEnumerable<TSignal> QueryAsync(IQueryRequest<TSignal> request, CancellationToken cancellationToken = default);
}