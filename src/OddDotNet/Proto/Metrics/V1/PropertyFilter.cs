using OddDotNet.Filters;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class PropertyFilter : IWhere<FlatMetric>
{
    public bool Matches(FlatMetric signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Name => StringFilter.Matches(signal.Metric.Name, Name),
        ValueOneofCase.Description => StringFilter.Matches(signal.Metric.Description, Description),
        ValueOneofCase.Unit => StringFilter.Matches(signal.Metric.Unit, Unit),
        ValueOneofCase.Metadata => KeyValueFilter.Matches(signal.Metric.Metadata, Metadata),
        ValueOneofCase.InstrumentationScopeSchemaUrl => StringFilter.Matches(signal.InstrumentationScopeSchemaUrl, InstrumentationScopeSchemaUrl),
        ValueOneofCase.ResourceSchemaUrl => StringFilter.Matches(signal.ResourceSchemaUrl, ResourceSchemaUrl),
        _ => false
    };
}