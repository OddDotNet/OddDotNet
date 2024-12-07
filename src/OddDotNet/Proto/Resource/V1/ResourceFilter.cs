using OddDotNet.Filters;

namespace OddDotNet.Proto.Resource.V1;

public sealed partial class ResourceFilter : IWhere<OpenTelemetry.Proto.Resource.V1.Resource>
{
    public bool Matches(OpenTelemetry.Proto.Resource.V1.Resource signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Attributes => KeyValueListFilter.Matches(signal.Attributes, Attributes),
        ValueOneofCase.DroppedAttributesCount => UInt32Filter.Matches(signal.DroppedAttributesCount, DroppedAttributesCount),
        _ => false
    };
}