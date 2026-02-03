using OddDotNet.Filters;
using OddDotNet.Filters.AppInsights;

namespace OddDotNet.Proto.AppInsights.V1;

public sealed partial class AvailabilityWhere : IWhere<FlatAvailability>
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

public sealed partial class AvailabilityPropertyFilter
{
    public bool Matches(AvailabilityTelemetry signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Id => StringFilter.Matches(signal.Id, Id),
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Duration => StringFilter.Matches(signal.Duration, Duration),
        ValueOneofCase.Success => BoolFilter.Matches(signal.Success, Success),
        ValueOneofCase.RunLocation => StringFilter.Matches(signal.RunLocation, RunLocation),
        ValueOneofCase.Message => StringFilter.Matches(signal.Message, Message),
        ValueOneofCase.Properties => PropertyMapFilterHelper.Matches(signal.Properties, Properties),
        ValueOneofCase.Measurements => MeasurementMapFilterHelper.Matches(signal.Measurements, Measurements),
        _ => false
    };
}

public sealed partial class AvailabilityOrFilter : IWhere<FlatAvailability>
{
    public bool Matches(FlatAvailability signal) => Filters.Any(filter => filter.Matches(signal));
}
