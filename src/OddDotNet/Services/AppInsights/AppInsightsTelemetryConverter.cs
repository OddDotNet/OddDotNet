using OddDotNet.Proto.AppInsights.V1;

namespace OddDotNet.Services.AppInsights;

/// <summary>
/// Converts App Insights JSON telemetry to proto flat telemetry types
/// </summary>
public static class AppInsightsTelemetryConverter
{
    public static TelemetryEnvelope CreateEnvelope(AppInsightsTelemetryEnvelope source)
    {
        var envelope = new TelemetryEnvelope
        {
            InstrumentationKey = source.InstrumentationKey,
            Time = source.Time,
            Context = CreateContext(source.Tags)
        };
        return envelope;
    }

    public static TelemetryContext CreateContext(Dictionary<string, string>? tags)
    {
        var context = new TelemetryContext
        {
            Operation = new OperationContext(),
            Cloud = new CloudContext(),
            Device = new DeviceContext(),
            User = new UserContext(),
            Session = new SessionContext(),
            Location = new LocationContext(),
            Application = new ApplicationContext(),
            Internal = new InternalContext()
        };

        if (tags == null) return context;

        foreach (var (key, value) in tags)
        {
            switch (key)
            {
                // Operation context
                case "ai.operation.id":
                    context.Operation.Id = value;
                    break;
                case "ai.operation.parentId":
                    context.Operation.ParentId = value;
                    break;
                case "ai.operation.name":
                    context.Operation.Name = value;
                    break;
                case "ai.operation.syntheticSource":
                    context.Operation.SyntheticSource = value;
                    break;
                case "ai.operation.correlationVector":
                    context.Operation.CorrelationVector = value;
                    break;
                
                // Cloud context
                case "ai.cloud.role":
                case "ai.cloud.roleName":
                    context.Cloud.RoleName = value;
                    break;
                case "ai.cloud.roleInstance":
                    context.Cloud.RoleInstance = value;
                    break;
                
                // Device context
                case "ai.device.id":
                    context.Device.Id = value;
                    break;
                case "ai.device.type":
                    context.Device.Type = value;
                    break;
                case "ai.device.osVersion":
                    context.Device.OsVersion = value;
                    break;
                
                // User context
                case "ai.user.id":
                    context.User.Id = value;
                    break;
                case "ai.user.authUserId":
                    context.User.AuthenticatedId = value;
                    break;
                case "ai.user.accountId":
                    context.User.AccountId = value;
                    break;
                
                // Session context
                case "ai.session.id":
                    context.Session.Id = value;
                    break;
                case "ai.session.isFirst":
                    context.Session.IsFirst = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                    break;
                
                // Location context
                case "ai.location.ip":
                    context.Location.Ip = value;
                    break;
                case "ai.location.country":
                    context.Location.Country = value;
                    break;
                case "ai.location.province":
                    context.Location.Province = value;
                    break;
                case "ai.location.city":
                    context.Location.City = value;
                    break;
                
                // Application context
                case "ai.application.ver":
                    context.Application.Version = value;
                    break;
                
                // Internal context
                case "ai.internal.sdkVersion":
                    context.Internal.SdkVersion = value;
                    break;
                case "ai.internal.agentVersion":
                    context.Internal.AgentVersion = value;
                    break;
                case "ai.internal.nodeName":
                    context.Internal.NodeName = value;
                    break;
            }
        }

        return context;
    }

    public static FlatRequest ToFlatRequest(AppInsightsTelemetryEnvelope source)
    {
        var baseData = source.Data?.BaseData;
        var request = new RequestTelemetry
        {
            Id = baseData?.Id ?? string.Empty,
            Name = baseData?.Name ?? string.Empty,
            Duration = baseData?.Duration ?? string.Empty,
            ResponseCode = baseData?.ResponseCode ?? string.Empty,
            Success = baseData?.Success ?? false,
            Url = baseData?.Url ?? string.Empty,
            Source = baseData?.Source ?? string.Empty
        };
        
        AddProperties(request.Properties, baseData?.Properties);
        AddMeasurements(request.Measurements, baseData?.Measurements);

        return new FlatRequest
        {
            Request = request,
            Envelope = CreateEnvelope(source)
        };
    }

    public static FlatDependency ToFlatDependency(AppInsightsTelemetryEnvelope source)
    {
        var baseData = source.Data?.BaseData;
        var dependency = new DependencyTelemetry
        {
            Id = baseData?.Id ?? string.Empty,
            Name = baseData?.Name ?? string.Empty,
            Duration = baseData?.Duration ?? string.Empty,
            ResultCode = baseData?.ResultCode ?? string.Empty,
            Success = baseData?.Success ?? false,
            Data = baseData?.Data ?? string.Empty,
            Target = baseData?.Target ?? string.Empty,
            Type = baseData?.Type ?? string.Empty
        };
        
        AddProperties(dependency.Properties, baseData?.Properties);
        AddMeasurements(dependency.Measurements, baseData?.Measurements);

        return new FlatDependency
        {
            Dependency = dependency,
            Envelope = CreateEnvelope(source)
        };
    }

    public static FlatException ToFlatException(AppInsightsTelemetryEnvelope source)
    {
        var baseData = source.Data?.BaseData;
        var exception = new ExceptionTelemetry
        {
            Id = baseData?.Id ?? string.Empty,
            ProblemId = baseData?.ProblemId ?? string.Empty,
            SeverityLevel = ToSeverityLevel(baseData?.SeverityLevel)
        };

        if (baseData?.Exceptions != null)
        {
            foreach (var ex in baseData.Exceptions)
            {
                var details = new ExceptionDetails
                {
                    Id = ex.Id,
                    OuterId = ex.OuterId,
                    TypeName = ex.TypeName ?? string.Empty,
                    Message = ex.Message ?? string.Empty,
                    HasFullStack = ex.HasFullStack,
                    Stack = ex.Stack ?? string.Empty
                };

                if (ex.ParsedStack != null)
                {
                    foreach (var frame in ex.ParsedStack)
                    {
                        details.ParsedStack.Add(new StackFrame
                        {
                            Level = frame.Level,
                            Method = frame.Method ?? string.Empty,
                            Assembly = frame.Assembly ?? string.Empty,
                            FileName = frame.FileName ?? string.Empty,
                            Line = frame.Line
                        });
                    }
                }

                exception.Exceptions.Add(details);
            }
        }
        
        AddProperties(exception.Properties, baseData?.Properties);
        AddMeasurements(exception.Measurements, baseData?.Measurements);

        return new FlatException
        {
            Exception = exception,
            Envelope = CreateEnvelope(source)
        };
    }

    public static FlatTrace ToFlatTrace(AppInsightsTelemetryEnvelope source)
    {
        var baseData = source.Data?.BaseData;
        var trace = new TraceTelemetry
        {
            Message = baseData?.Message ?? string.Empty,
            SeverityLevel = ToSeverityLevel(baseData?.SeverityLevel)
        };
        
        AddProperties(trace.Properties, baseData?.Properties);

        return new FlatTrace
        {
            Trace = trace,
            Envelope = CreateEnvelope(source)
        };
    }

    public static FlatEvent ToFlatEvent(AppInsightsTelemetryEnvelope source)
    {
        var baseData = source.Data?.BaseData;
        var evt = new EventTelemetry
        {
            Name = baseData?.Name ?? string.Empty
        };
        
        AddProperties(evt.Properties, baseData?.Properties);
        AddMeasurements(evt.Measurements, baseData?.Measurements);

        return new FlatEvent
        {
            Event = evt,
            Envelope = CreateEnvelope(source)
        };
    }

    public static IEnumerable<FlatMetric> ToFlatMetrics(AppInsightsTelemetryEnvelope source)
    {
        var baseData = source.Data?.BaseData;
        var envelope = CreateEnvelope(source);

        if (baseData?.Metrics != null)
        {
            foreach (var metricData in baseData.Metrics)
            {
                var metric = new MetricTelemetry
                {
                    Name = metricData.Name,
                    MetricNamespace = metricData.Namespace ?? string.Empty,
                    Value = metricData.Value,
                    Count = metricData.Count ?? 1
                };

                if (metricData.Min.HasValue)
                    metric.Min = metricData.Min.Value;
                if (metricData.Max.HasValue)
                    metric.Max = metricData.Max.Value;
                if (metricData.StdDev.HasValue)
                    metric.StdDev = metricData.StdDev.Value;

                AddProperties(metric.Properties, baseData.Properties);

                yield return new FlatMetric
                {
                    Metric = metric,
                    Envelope = envelope
                };
            }
        }
    }

    public static FlatPageView ToFlatPageView(AppInsightsTelemetryEnvelope source)
    {
        var baseData = source.Data?.BaseData;
        var pageView = new PageViewTelemetry
        {
            Id = baseData?.Id ?? string.Empty,
            Name = baseData?.Name ?? string.Empty,
            Url = baseData?.Url ?? string.Empty,
            Duration = baseData?.Duration ?? string.Empty,
            ReferrerUri = baseData?.ReferrerUri ?? string.Empty
        };
        
        AddProperties(pageView.Properties, baseData?.Properties);
        AddMeasurements(pageView.Measurements, baseData?.Measurements);

        return new FlatPageView
        {
            PageView = pageView,
            Envelope = CreateEnvelope(source)
        };
    }

    public static FlatAvailability ToFlatAvailability(AppInsightsTelemetryEnvelope source)
    {
        var baseData = source.Data?.BaseData;
        var availability = new AvailabilityTelemetry
        {
            Id = baseData?.Id ?? string.Empty,
            Name = baseData?.Name ?? string.Empty,
            Duration = baseData?.Duration ?? string.Empty,
            Success = baseData?.Success ?? false,
            RunLocation = baseData?.RunLocation ?? string.Empty,
            Message = baseData?.Message ?? string.Empty
        };
        
        AddProperties(availability.Properties, baseData?.Properties);
        AddMeasurements(availability.Measurements, baseData?.Measurements);

        return new FlatAvailability
        {
            Availability = availability,
            Envelope = CreateEnvelope(source)
        };
    }

    private static SeverityLevel ToSeverityLevel(int? level) => level switch
    {
        0 => SeverityLevel.Verbose,
        1 => SeverityLevel.Information,
        2 => SeverityLevel.Warning,
        3 => SeverityLevel.Error,
        4 => SeverityLevel.Critical,
        _ => SeverityLevel.Unspecified
    };

    private static void AddProperties(Google.Protobuf.Collections.MapField<string, string> target, 
        Dictionary<string, string>? source)
    {
        if (source == null) return;
        foreach (var (key, value) in source)
        {
            target[key] = value;
        }
    }

    private static void AddMeasurements(Google.Protobuf.Collections.MapField<string, double> target, 
        Dictionary<string, double>? source)
    {
        if (source == null) return;
        foreach (var (key, value) in source)
        {
            target[key] = value;
        }
    }
}
