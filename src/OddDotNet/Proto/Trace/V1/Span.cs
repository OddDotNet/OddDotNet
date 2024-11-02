using OddDotNet;

namespace OpenTelemetry.Proto.Trace.V1;

public sealed partial class Span : ISignal
{
    public static partial class Types
    {
        public sealed partial class Event : ISignal
        {
        }

        public sealed partial class Link : ISignal
        {
        }
    }
}