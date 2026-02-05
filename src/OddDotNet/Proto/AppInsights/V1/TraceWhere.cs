using OddDotNet.Filters;
using OddDotNet.Filters.AppInsights;

namespace OddDotNet.Proto.AppInsights.V1.Trace;

public sealed partial class Where : IWhere<FlatTrace>
{
    public bool Matches(FlatTrace signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal.Trace),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.Envelope => Envelope.Matches(signal.Envelope),
        _ => false
    };
}

public sealed partial class PropertyFilter
{
    public bool Matches(V1.TraceTelemetry signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Message => StringFilter.Matches(signal.Message, Message),
        ValueOneofCase.SeverityLevel => SeverityLevelFilter.Matches(signal.SeverityLevel, SeverityLevel),
        ValueOneofCase.Properties => PropertyMapFilter.Matches(signal.Properties, Properties),
        _ => false
    };
}

public sealed partial class OrFilter : IWhere<FlatTrace>
{
    public bool Matches(FlatTrace signal) => Filters.Any(filter => filter.Matches(signal));
}
