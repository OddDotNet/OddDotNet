using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class ExemplarFilter : IWhere<Exemplar>
{
    public bool Matches(Exemplar signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.FilteredAttribute => KeyValueFilter.Matches(signal.FilteredAttributes, FilteredAttribute)
    };
}