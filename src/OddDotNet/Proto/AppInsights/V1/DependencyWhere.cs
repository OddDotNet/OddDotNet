using OddDotNet.Filters;
using OddDotNet.Filters.AppInsights;

namespace OddDotNet.Proto.AppInsights.V1.Dependency;

public sealed partial class Where : IWhere<FlatDependency>
{
    public bool Matches(FlatDependency signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal.Dependency),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.Envelope => Envelope.Matches(signal.Envelope),
        _ => false
    };
}

public sealed partial class PropertyFilter
{
    public bool Matches(V1.DependencyTelemetry signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Id => StringFilter.Matches(signal.Id, Id),
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Duration => StringFilter.Matches(signal.Duration, Duration),
        ValueOneofCase.ResultCode => StringFilter.Matches(signal.ResultCode, ResultCode),
        ValueOneofCase.Success => BoolFilter.Matches(signal.Success, Success),
        ValueOneofCase.Data => StringFilter.Matches(signal.Data, Data),
        ValueOneofCase.Target => StringFilter.Matches(signal.Target, Target),
        ValueOneofCase.Type => StringFilter.Matches(signal.Type, Type),
        ValueOneofCase.Properties => PropertyMapFilterHelper.Matches(signal.Properties, Properties),
        ValueOneofCase.Measurements => MeasurementMapFilterHelper.Matches(signal.Measurements, Measurements),
        _ => false
    };
}

public sealed partial class OrFilter : IWhere<FlatDependency>
{
    public bool Matches(FlatDependency signal) => Filters.Any(filter => filter.Matches(signal));
}
