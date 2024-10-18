using System.Runtime.CompilerServices;

namespace OddDotNet;

public interface ISignalList<TSignal> : ISignalList where TSignal : class
{
    void Add(TSignal signal);
    IAsyncEnumerable<TSignal> QueryAsync(IQueryRequest<TSignal> request, CancellationToken cancellationToken = default);
    void Reset(IResetRequest<TSignal> request);
}

public interface ISignalList
{
    void Prune();
}