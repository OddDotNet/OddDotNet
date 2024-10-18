namespace OddDotNet;

public sealed record OddSettings
{
    public const string CacheExpirationEnvVarName = "ODD_CACHE_EXPIRATION";
    public const string CacheCleanupIntervalEnvVarName = "ODD_CACHE_CLEANUP_INTERVAL";
    public CacheSettings Cache { get; init; } = new();
}

public sealed record CacheSettings
{
    public uint Expiration { get; set; } = 30000;
    public uint CleanupInterval { get; set; } = 1000;
}