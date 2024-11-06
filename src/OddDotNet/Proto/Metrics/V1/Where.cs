using OddDotNet.Filters;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class Where : IWhere<FlatMetric>
{
    public bool Matches(FlatMetric signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal.Metric),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.InstrumentationScope => InstrumentationScope.Matches(signal.InstrumentationScope),
        ValueOneofCase.Resource => Resource.Matches(signal.Resource),
        ValueOneofCase.InstrumentationScopeSchemaUrl => StringFilter.Matches(signal.InstrumentationScopeSchemaUrl, InstrumentationScopeSchemaUrl),
        ValueOneofCase.ResourceSchemaUrl => StringFilter.Matches(signal.ResourceSchemaUrl, ResourceSchemaUrl),
        _ => false
    };
}