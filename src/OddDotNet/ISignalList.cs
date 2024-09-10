namespace OddDotNet;

public interface ISignalList<TSignal> where TSignal : class
{
    void Add(TSignal signal);
    List<TSignal> Query(IQueryRequest<TSignal> request);
}