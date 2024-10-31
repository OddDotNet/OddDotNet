namespace OddDotNet;

public interface IWhere<in TSignal> where TSignal : ISignal
{
    bool Matches(TSignal signal);
}