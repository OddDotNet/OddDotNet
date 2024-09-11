namespace OddDotNet;

public record Expirable<TSignal>(TSignal Signal, DateTimeOffset ExpireAt)
    where TSignal : class;