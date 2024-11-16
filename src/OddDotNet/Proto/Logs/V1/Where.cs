using OddDotNet.Filters;

namespace OddDotNet.Proto.Logs.V1;

public sealed partial class Where : IWhere<FlatLog>
{
    public bool Matches(FlatLog signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal.Log),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.InstrumentationScope => InstrumentationScope.Matches(signal.InstrumentationScope),
        ValueOneofCase.Resource => Resource.Matches(signal.Resource),
        ValueOneofCase.ResourceSchemaUrl => StringFilter.Matches(signal.ResourceSchemaUrl, ResourceSchemaUrl),
        ValueOneofCase.InstrumentationScopeSchemaUrl => StringFilter.Matches(signal.InstrumentationScopeSchemaUrl, InstrumentationScopeSchemaUrl)
    };
}