using OddDotNet.Filters;
using OddDotNet.Filters.AppInsights;

namespace OddDotNet.Proto.AppInsights.V1;

public sealed partial class RequestWhere : IWhere<FlatRequest>
{
    public bool Matches(FlatRequest signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal.Request),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.Envelope => Envelope.Matches(signal.Envelope),
        _ => false
    };
}

public sealed partial class RequestPropertyFilter
{
    public bool Matches(RequestTelemetry signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Id => StringFilter.Matches(signal.Id, Id),
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Duration => StringFilter.Matches(signal.Duration, Duration),
        ValueOneofCase.ResponseCode => StringFilter.Matches(signal.ResponseCode, ResponseCode),
        ValueOneofCase.Success => BoolFilter.Matches(signal.Success, Success),
        ValueOneofCase.Url => StringFilter.Matches(signal.Url, Url),
        ValueOneofCase.Source => StringFilter.Matches(signal.Source, Source),
        ValueOneofCase.Properties => PropertyMapFilterHelper.Matches(signal.Properties, Properties),
        ValueOneofCase.Measurements => MeasurementMapFilterHelper.Matches(signal.Measurements, Measurements),
        _ => false
    };
}

public sealed partial class RequestOrFilter : IWhere<FlatRequest>
{
    public bool Matches(FlatRequest signal) => Filters.Any(filter => filter.Matches(signal));
}
