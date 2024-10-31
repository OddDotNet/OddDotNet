using OddDotNet.Proto.Common.V1;

namespace OddDotNet;

public interface IQueryRequest<TSignal, TFilter> where TSignal : class
{
    Take Take { get; set; }
    Duration Duration { get; set; }
}