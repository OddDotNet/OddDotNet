namespace OddDotNet;

public interface ISignalList<in TSignal> where TSignal : class
{
    void Add(TSignal signal);
}