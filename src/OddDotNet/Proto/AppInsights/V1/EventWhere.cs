using OddDotNet.Filters;
using OddDotNet.Filters.AppInsights;

namespace OddDotNet.Proto.AppInsights.V1;

public sealed partial class EventWhere : IWhere<FlatEvent>
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

public sealed partial class EventPropertyFilter
{
    public bool Matches(EventTelemetry signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Properties => PropertyMapFilterHelper.Matches(signal.Properties, Properties),
        ValueOneofCase.Measurements => MeasurementMapFilterHelper.Matches(signal.Measurements, Measurements),
        _ => false
    };
}

public sealed partial class EventOrFilter : IWhere<FlatEvent>
{
    public bool Matches(FlatEvent signal) => Filters.Any(filter => filter.Matches(signal));
}
