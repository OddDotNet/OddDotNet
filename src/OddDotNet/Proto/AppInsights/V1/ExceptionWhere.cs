using OddDotNet.Filters;
using OddDotNet.Filters.AppInsights;

namespace OddDotNet.Proto.AppInsights.V1.Exception;

public sealed partial class Where : IWhere<FlatException>
{
    public bool Matches(FlatException signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal.Exception),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.Envelope => Envelope.Matches(signal.Envelope),
        _ => false
    };
}

public sealed partial class PropertyFilter
{
    public bool Matches(V1.ExceptionTelemetry signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Id => StringFilter.Matches(signal.Id, Id),
        ValueOneofCase.ProblemId => StringFilter.Matches(signal.ProblemId, ProblemId),
        ValueOneofCase.SeverityLevel => SeverityLevelFilter.Matches(signal.SeverityLevel, SeverityLevel),
        ValueOneofCase.ExceptionDetails => signal.Exceptions.Any(ex => ExceptionDetails.Matches(ex)),
        ValueOneofCase.Properties => PropertyMapFilter.Matches(signal.Properties, Properties),
        ValueOneofCase.Measurements => MeasurementMapFilter.Matches(signal.Measurements, Measurements),
        _ => false
    };
}

public sealed partial class ExceptionDetailsFilter
{
    public bool Matches(V1.ExceptionDetails details) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.TypeName => StringFilter.Matches(details.TypeName, TypeName),
        ValueOneofCase.Message => StringFilter.Matches(details.Message, Message),
        ValueOneofCase.Stack => StringFilter.Matches(details.Stack, Stack),
        ValueOneofCase.OuterId => Int32Filter.Matches(details.OuterId, OuterId),
        _ => false
    };
}

public sealed partial class OrFilter : IWhere<FlatException>
{
    public bool Matches(FlatException signal) => Filters.Any(filter => filter.Matches(signal));
}
