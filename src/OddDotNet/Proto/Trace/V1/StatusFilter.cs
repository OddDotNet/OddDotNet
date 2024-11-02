using OddDotNet.Filters;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Proto.Trace.V1;

public sealed partial class StatusFilter : IWhere<Status>
{
    public bool Matches(Status signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Message => StringFilter.Matches(signal.Message, Message),
        ValueOneofCase.Code => StatusCodeFilter.Matches(signal.Code, Code),
        _ => false
    };
}