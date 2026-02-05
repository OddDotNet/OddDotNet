using OddDotNet.Filters;
using OddDotNet.Filters.AppInsights;

namespace OddDotNet.Proto.AppInsights.V1.Availability;

public sealed partial class Where : IWhere<FlatAvailability>
{
    public bool Matches(FlatAvailability signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal.Availability),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.Envelope => Envelope.Matches(signal.Envelope),
        _ => false
    };
}

public sealed partial class PropertyFilter
{
    public bool Matches(V1.AvailabilityTelemetry signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Id => StringFilter.Matches(signal.Id, Id),
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Duration => StringFilter.Matches(signal.Duration, Duration),
        ValueOneofCase.Success => BoolFilter.Matches(signal.Success, Success),
        ValueOneofCase.RunLocation => StringFilter.Matches(signal.RunLocation, RunLocation),
        ValueOneofCase.Message => StringFilter.Matches(signal.Message, Message),
        ValueOneofCase.Properties => PropertyMapFilter.Matches(signal.Properties, Properties),
        ValueOneofCase.Measurements => MeasurementMapFilter.Matches(signal.Measurements, Measurements),
        _ => false
    };
}

public sealed partial class OrFilter : IWhere<FlatAvailability>
{
    public bool Matches(FlatAvailability signal) => Filters.Any(filter => filter.Matches(signal));
}
