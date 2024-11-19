using OddDotNet.Filters;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Proto.Metrics.V1;

public sealed partial class PropertyFilter : IWhere<Metric>
{
    public bool Matches(Metric signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Description => StringFilter.Matches(signal.Description, Description),
        ValueOneofCase.Unit => StringFilter.Matches(signal.Unit, Unit),
        ValueOneofCase.Gauge => Gauge.Matches(signal.Gauge),
        ValueOneofCase.Sum => Sum.Matches(signal.Sum),
        ValueOneofCase.Histogram => Histogram.Matches(signal.Histogram),
        ValueOneofCase.ExponentialHistogram => ExponentialHistogram.Matches(signal.ExponentialHistogram),
        ValueOneofCase.Summary => Summary.Matches(signal.Summary),
        ValueOneofCase.Metadata => KeyValueListFilter.Matches(signal.Metadata, Metadata),
        _ => false
    };
}