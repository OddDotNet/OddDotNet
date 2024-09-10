namespace OddDotNet;

public class Span
{
    public required Scope Scope { get; set; }
    public required string Name { get; set; }
    public required Dictionary<string, object> Attributes { get; set; }
    public List<SpanEvent>? Events { get; set; }
    public uint Flags { get; set; }
    public SpanKind Kind { get; set; } = SpanKind.Internal;
    public List<SpanLink>? Links { get; set; }
    public SpanStatus Status { get; set; } = new SpanStatus { Code = SpanStatusCode.Unset };
    public required byte[] SpanId { get; set; }
    public required byte[] TraceId { get; set; }
    public required string TraceState { get; set; }
    public byte[]? ParentSpanId { get; set; }
    public ulong StartTimeUnixNano { get; set; }
    public ulong EndTimeUnixNano { get; set; }
}

public class SpanStatus
{
    public SpanStatusCode Code { get; set; }
    public string? Message { get; set; }
}

public enum SpanStatusCode
{
    Unset = 0,
    Ok = 1,
    Error = 2
}

public class SpanLink
{
    public required byte[] SpanId { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
    public uint Flags { get; set; }
    public byte[]? TraceId { get; set; }
    public required string TraceState { get; set; }
}

public enum SpanKind
{
    Unspecified = 0,
    Internal = 1,
    Server = 2,
    Client = 3,
    Producer = 4,
    Consumer = 5
}

public class SpanEvent
{
    public required string Name { get; set; }
    public required Dictionary<string, object> Attributes { get; set; }
    public uint TimeUnixNano { get; set; }
}

public class Scope
{
    public string? Name { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
    public string? Version { get; set; }
    public string? SchemaUrl { get; set; }
    public required Resource Resource { get; set; }
}

public class Resource
{
    public required Dictionary<string, object> Attributes { get; set; }
    public string? SchemaUrl { get; set; }
}