using OddDotNet.Filters;
using OddDotNet.Filters.AppInsights;

namespace OddDotNet.Proto.AppInsights.V1.Event;

public sealed partial class Where : IWhere<FlatEvent>
{
    public bool Matches(FlatEvent signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal.Event),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.Envelope => Envelope.Matches(signal.Envelope),
        _ => false
    };
}

public sealed partial class PropertyFilter
{
    public bool Matches(V1.EventTelemetry signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Properties => PropertyMapPropertyHelper.Matches(signal.Properties, Properties),
        ValueOneofCase.Measurements => MeasurementMapPropertyHelper.Matches(signal.Measurements, Measurements),
        _ => false
    };
}

public sealed partial class OrFilter : IWhere<FlatEvent>
{
    public bool Matches(FlatEvent signal) => Filters.Any(filter => filter.Matches(signal));
}
