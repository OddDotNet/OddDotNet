using OddDotNet.Filters;
using OddDotNet.Filters.AppInsights;

namespace OddDotNet.Proto.AppInsights.V1;

public sealed partial class MetricWhere : IWhere<FlatMetric>
{
    public bool Matches(FlatMetric signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal.Metric),
        ValueOneofCase.Or => Or.Matches(signal),
        ValueOneofCase.Envelope => Envelope.Matches(signal.Envelope),
        _ => false
    };
}

public sealed partial class MetricPropertyFilter
{
    public bool Matches(MetricTelemetry signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.MetricNamespace => StringFilter.Matches(signal.MetricNamespace, MetricNamespace),
        ValueOneofCase.MetricValue => DoubleFilter.Matches(signal.Value, MetricValue),
        ValueOneofCase.Count => Int32Filter.Matches(signal.Count, Count),
        ValueOneofCase.Min => signal.HasMin && DoubleFilter.Matches(signal.Min, Min),
        ValueOneofCase.Max => signal.HasMax && DoubleFilter.Matches(signal.Max, Max),
        ValueOneofCase.StdDev => signal.HasStdDev && DoubleFilter.Matches(signal.StdDev, StdDev),
        ValueOneofCase.Properties => PropertyMapFilterHelper.Matches(signal.Properties, Properties),
        _ => false
    };
}

public sealed partial class MetricOrFilter : IWhere<FlatMetric>
{
    public bool Matches(FlatMetric signal) => Filters.Any(filter => filter.Matches(signal));
}
