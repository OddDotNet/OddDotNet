namespace OddDotNet;

public record Span
{
    public required Scope Scope { get; init; }
    public required string Name { get; init; }
    public Dictionary<string, object> Attributes { get; } = new();
    public List<SpanEvent> Events { get; } = new();
    public required uint Flags { get; init; }
    public required SpanKind Kind { get; init; }
    public List<SpanLink> Links { get; } = new();
    public required SpanStatus Status { get; init; }
    public required byte[] SpanId { get; init; }
    public required byte[] TraceId { get; init; }
    public required string TraceState { get; init; }
    public byte[] ParentSpanId { get; init; } = [];
    public required ulong StartTimeUnixNano { get; init; }
    public required ulong EndTimeUnixNano { get; init; }
}

public record SpanStatus
{
    public required SpanStatusCode Code { get; init; }
    public string? Message { get; init; }
}

public enum SpanStatusCode
{
    Unset = 0,
    Ok = 1,
    Error = 2
}

public record SpanLink
{
    public required byte[] SpanId { get; init; }
    public Dictionary<string, object> Attributes { get; } = new();
    public required uint Flags { get; init; }
    public required byte[] TraceId { get; init; } = [];
    public required string TraceState { get; init; }
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

public record SpanEvent
{
    public required string Name { get; init; }
    public Dictionary<string, object> Attributes { get; } = new();
    public required uint TimeUnixNano { get; init; }
}

public record Scope
{
    public required string Name { get; init; }
    public Dictionary<string, object> Attributes { get; } = new();
    public string? Version { get; init; }
    public string? SchemaUrl { get; init; }
    public required Resource Resource { get; init; }
}

public record Resource
{
    public required Dictionary<string, object> Attributes { get; init; }
    public string? SchemaUrl { get; init; }
}