using System.Text.Json.Serialization;

namespace OddDotNet.Services.AppInsights;

/// <summary>
/// Represents the App Insights telemetry envelope structure sent to /v2/track
/// </summary>
public class AppInsightsTelemetryEnvelope
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;
    
    [JsonPropertyName("iKey")]
    public string InstrumentationKey { get; set; } = string.Empty;
    
    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
    
    [JsonPropertyName("data")]
    public AppInsightsData? Data { get; set; }
}

public class AppInsightsData
{
    [JsonPropertyName("baseType")]
    public string BaseType { get; set; } = string.Empty;
    
    [JsonPropertyName("baseData")]
    public AppInsightsBaseData? BaseData { get; set; }
}

/// <summary>
/// Union type for all App Insights base data types
/// </summary>
public class AppInsightsBaseData
{
    // Common fields
    [JsonPropertyName("ver")]
    public int Version { get; set; } = 2;
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("properties")]
    public Dictionary<string, string>? Properties { get; set; }
    
    [JsonPropertyName("measurements")]
    public Dictionary<string, double>? Measurements { get; set; }
    
    // Request-specific fields
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }
    
    [JsonPropertyName("responseCode")]
    public string? ResponseCode { get; set; }
    
    [JsonPropertyName("success")]
    public bool? Success { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    
    // Dependency-specific fields
    [JsonPropertyName("resultCode")]
    public string? ResultCode { get; set; }
    
    [JsonPropertyName("data")]
    public string? Data { get; set; }
    
    [JsonPropertyName("target")]
    public string? Target { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    // Exception-specific fields
    [JsonPropertyName("problemId")]
    public string? ProblemId { get; set; }
    
    [JsonPropertyName("exceptions")]
    public List<AppInsightsExceptionDetails>? Exceptions { get; set; }
    
    [JsonPropertyName("severityLevel")]
    public int? SeverityLevel { get; set; }
    
    // Trace-specific fields
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    // Metric-specific fields
    [JsonPropertyName("metrics")]
    public List<AppInsightsMetricData>? Metrics { get; set; }
    
    // PageView-specific fields
    [JsonPropertyName("referrerUri")]
    public string? ReferrerUri { get; set; }
    
    // Availability-specific fields
    [JsonPropertyName("runLocation")]
    public string? RunLocation { get; set; }
}

public class AppInsightsExceptionDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("outerId")]
    public int OuterId { get; set; }
    
    [JsonPropertyName("typeName")]
    public string? TypeName { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("hasFullStack")]
    public bool HasFullStack { get; set; }
    
    [JsonPropertyName("stack")]
    public string? Stack { get; set; }
    
    [JsonPropertyName("parsedStack")]
    public List<AppInsightsStackFrame>? ParsedStack { get; set; }
}

public class AppInsightsStackFrame
{
    [JsonPropertyName("level")]
    public int Level { get; set; }
    
    [JsonPropertyName("method")]
    public string? Method { get; set; }
    
    [JsonPropertyName("assembly")]
    public string? Assembly { get; set; }
    
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
    
    [JsonPropertyName("line")]
    public int Line { get; set; }
}

public class AppInsightsMetricData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("ns")]
    public string? Namespace { get; set; }
    
    [JsonPropertyName("value")]
    public double Value { get; set; }
    
    [JsonPropertyName("count")]
    public int? Count { get; set; }
    
    [JsonPropertyName("min")]
    public double? Min { get; set; }
    
    [JsonPropertyName("max")]
    public double? Max { get; set; }
    
    [JsonPropertyName("stdDev")]
    public double? StdDev { get; set; }
}
