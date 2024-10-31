namespace OddDotNet.Proto.Trace.V1;

public sealed partial class WhereFilter : IWhere<FlatSpan>
{
    public bool Matches(FlatSpan signal) => ValueCase switch
    {
        ValueOneofCase.None => false,
        ValueOneofCase.Property => Property.Matches(signal),
        ValueOneofCase.Or => Or.Matches(signal),
        _ => false
    };
}