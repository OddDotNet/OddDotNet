using OddDotNet.Filters;
using OpenTelemetry.Proto.Common.V1;

namespace OddDotNet.Proto.Common.V1;

public sealed partial class InstrumentationScopeFilter : IWhere<InstrumentationScope>
{
    public bool Matches(InstrumentationScope signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Name => StringFilter.Matches(signal.Name, Name),
        ValueOneofCase.Attributes => KeyValueListFilter.Matches(signal.Attributes, Attributes),
        ValueOneofCase.Version => StringFilter.Matches(signal.Version, Version),
        ValueOneofCase.DroppedAttributesCount => UInt32Filter.Matches(signal.DroppedAttributesCount, DroppedAttributesCount),
        _ => false
    };
}