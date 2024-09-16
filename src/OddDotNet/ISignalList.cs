namespace OddDotNet;

public interface ISignalList<TSignal> where TSignal : class
{
    void Add(TSignal signal);
    Task<List<TSignal>> QueryAsync(IQueryRequest<TSignal> request, CancellationToken cancellationToken);
}