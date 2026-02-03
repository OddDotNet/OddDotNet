using Bogus;
using OddDotNet.Proto.AppInsights.V1;
using OddDotNet.Proto.AppInsights.V1.Availability;
using OddDotNet.Proto.AppInsights.V1.Dependency;
using OddDotNet.Proto.AppInsights.V1.Event;
using OddDotNet.Proto.AppInsights.V1.Exception;
using OddDotNet.Proto.AppInsights.V1.Metric;
using OddDotNet.Proto.AppInsights.V1.PageView;
using OddDotNet.Proto.AppInsights.V1.Request;
using OddDotNet.Proto.AppInsights.V1.Trace;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests;

/// <summary>
/// Helper methods for generating test App Insights telemetry data
/// </summary>
public static class AppInsightsHelpers
{
    private static readonly Faker Faker = new();

    #region Envelope and Context

    public static TelemetryEnvelope CreateTelemetryEnvelope()
    {
        return new TelemetryEnvelope
        {
            InstrumentationKey = Faker.Random.Guid().ToString(),
            Time = DateTime.UtcNow.ToString("O"),
            Context = CreateTelemetryContext()
        };
    }

    public static TelemetryContext CreateTelemetryContext()
    {
        return new TelemetryContext
        {
            Operation = CreateOperationContext(),
            Cloud = CreateCloudContext(),
            Device = CreateDeviceContext(),
            User = CreateUserContext(),
            Session = CreateSessionContext(),
            Location = CreateLocationContext(),
            Application = CreateApplicationContext(),
            Internal = CreateInternalContext()
        };
    }

    public static OperationContext CreateOperationContext()
    {
        return new OperationContext
        {
            Id = Faker.Random.Guid().ToString(),
            ParentId = Faker.Random.Guid().ToString(),
            Name = $"{Faker.PickRandom("GET", "POST", "PUT", "DELETE")} /api/{Faker.Random.String2(8)}",
            SyntheticSource = Faker.Random.Bool() ? "Availability" : "",
            CorrelationVector = Faker.Random.String2(16)
        };
    }

    public static CloudContext CreateCloudContext()
    {
        return new CloudContext
        {
            RoleName = Faker.Commerce.ProductName().Replace(" ", ""),
            RoleInstance = $"{Faker.Random.String2(8)}-instance-{Faker.Random.Int(1, 10)}"
        };
    }

    public static DeviceContext CreateDeviceContext()
    {
        return new DeviceContext
        {
            Id = Faker.Random.Guid().ToString(),
            Type = Faker.PickRandom("PC", "Phone", "Tablet", "Browser"),
            OsVersion = $"{Faker.Random.Int(10, 14)}.{Faker.Random.Int(0, 5)}.{Faker.Random.Int(0, 1000)}"
        };
    }

    public static UserContext CreateUserContext()
    {
        return new UserContext
        {
            Id = Faker.Random.Guid().ToString(),
            AuthenticatedId = Faker.Internet.Email(),
            AccountId = Faker.Random.Guid().ToString()
        };
    }

    public static SessionContext CreateSessionContext()
    {
        return new SessionContext
        {
            Id = Faker.Random.Guid().ToString(),
            IsFirst = Faker.Random.Bool()
        };
    }

    public static LocationContext CreateLocationContext()
    {
        return new LocationContext
        {
            Ip = Faker.Internet.Ip(),
            Country = Faker.Address.CountryCode(),
            Province = Faker.Address.State(),
            City = Faker.Address.City()
        };
    }

    public static ApplicationContext CreateApplicationContext()
    {
        return new ApplicationContext
        {
            Version = $"{Faker.Random.Int(1, 5)}.{Faker.Random.Int(0, 99)}.{Faker.Random.Int(0, 999)}"
        };
    }

    public static InternalContext CreateInternalContext()
    {
        return new InternalContext
        {
            SdkVersion = $"dotnet:{Faker.Random.Int(2, 5)}.{Faker.Random.Int(0, 20)}.{Faker.Random.Int(0, 10)}",
            AgentVersion = Faker.Random.String2(8),
            NodeName = Faker.Internet.DomainWord()
        };
    }

    #endregion

    #region Request Telemetry

    public static FlatRequest CreateFlatRequest()
    {
        return new FlatRequest
        {
            Request = CreateRequestTelemetry(),
            Envelope = CreateTelemetryEnvelope()
        };
    }

    public static RequestTelemetry CreateRequestTelemetry()
    {
        var success = Faker.Random.Bool(0.9f);
        var request = new RequestTelemetry
        {
            Id = Faker.Random.Guid().ToString(),
            Name = $"{Faker.PickRandom("GET", "POST", "PUT", "DELETE")} /api/{Faker.Random.String2(8)}",
            Duration = $"00:00:0{Faker.Random.Int(0, 9)}.{Faker.Random.Int(100, 999)}",
            ResponseCode = success ? "200" : Faker.PickRandom("400", "401", "403", "404", "500"),
            Success = success,
            Url = Faker.Internet.Url(),
            Source = Faker.Internet.Ip()
        };
        AddProperties(request.Properties);
        AddMeasurements(request.Measurements);
        return request;
    }

    public static AppInsightsTelemetryEnvelope CreateRequestEnvelope()
    {
        var success = Faker.Random.Bool(0.9f);
        return new AppInsightsTelemetryEnvelope
        {
            Name = "Microsoft.ApplicationInsights.Request",
            Time = DateTime.UtcNow.ToString("O"),
            InstrumentationKey = Faker.Random.Guid().ToString(),
            Tags = CreateTags(),
            Data = new AppInsightsData
            {
                BaseType = "RequestData",
                BaseData = new AppInsightsBaseData
                {
                    Id = Faker.Random.Guid().ToString(),
                    Name = $"{Faker.PickRandom("GET", "POST")} /api/{Faker.Random.String2(8)}",
                    Duration = $"00:00:0{Faker.Random.Int(0, 9)}.{Faker.Random.Int(100, 999)}",
                    ResponseCode = success ? "200" : "500",
                    Success = success,
                    Url = Faker.Internet.Url(),
                    Properties = CreatePropertiesDict(),
                    Measurements = CreateMeasurementsDict()
                }
            }
        };
    }

    #endregion

    #region Dependency Telemetry

    public static FlatDependency CreateFlatDependency()
    {
        return new FlatDependency
        {
            Dependency = CreateDependencyTelemetry(),
            Envelope = CreateTelemetryEnvelope()
        };
    }

    public static DependencyTelemetry CreateDependencyTelemetry()
    {
        var success = Faker.Random.Bool(0.95f);
        var dependency = new DependencyTelemetry
        {
            Id = Faker.Random.Guid().ToString(),
            Name = $"{Faker.PickRandom("GET", "POST")} /{Faker.Random.String2(8)}",
            Duration = $"00:00:0{Faker.Random.Int(0, 5)}.{Faker.Random.Int(100, 999)}",
            ResultCode = success ? "200" : "500",
            Success = success,
            Data = Faker.Internet.Url(),
            Target = Faker.Internet.DomainName(),
            Type = Faker.PickRandom("HTTP", "SQL", "Azure Blob", "Azure Table", "Redis")
        };
        AddProperties(dependency.Properties);
        AddMeasurements(dependency.Measurements);
        return dependency;
    }

    public static AppInsightsTelemetryEnvelope CreateDependencyEnvelope()
    {
        var success = Faker.Random.Bool(0.95f);
        return new AppInsightsTelemetryEnvelope
        {
            Name = "Microsoft.ApplicationInsights.RemoteDependency",
            Time = DateTime.UtcNow.ToString("O"),
            InstrumentationKey = Faker.Random.Guid().ToString(),
            Tags = CreateTags(),
            Data = new AppInsightsData
            {
                BaseType = "RemoteDependencyData",
                BaseData = new AppInsightsBaseData
                {
                    Id = Faker.Random.Guid().ToString(),
                    Name = $"GET /{Faker.Random.String2(8)}",
                    Duration = $"00:00:0{Faker.Random.Int(0, 5)}.{Faker.Random.Int(100, 999)}",
                    ResultCode = success ? "200" : "500",
                    Success = success,
                    Data = Faker.Internet.Url(),
                    Target = Faker.Internet.DomainName(),
                    Type = "HTTP"
                }
            }
        };
    }

    #endregion

    #region Exception Telemetry

    public static FlatException CreateFlatException()
    {
        return new FlatException
        {
            Exception = CreateExceptionTelemetry(),
            Envelope = CreateTelemetryEnvelope()
        };
    }

    public static ExceptionTelemetry CreateExceptionTelemetry()
    {
        var exception = new ExceptionTelemetry
        {
            Id = Faker.Random.Guid().ToString(),
            ProblemId = $"{Faker.Random.String2(8)}_{Faker.Random.String2(8)}",
            SeverityLevel = Faker.PickRandom<SeverityLevel>()
        };
        exception.Exceptions.Add(CreateExceptionDetails());
        AddProperties(exception.Properties);
        AddMeasurements(exception.Measurements);
        return exception;
    }

    public static ExceptionDetails CreateExceptionDetails()
    {
        var details = new ExceptionDetails
        {
            Id = Faker.Random.Int(1, 100),
            OuterId = 0,
            TypeName = Faker.PickRandom("System.NullReferenceException", "System.ArgumentException", "System.InvalidOperationException"),
            Message = Faker.Lorem.Sentence(),
            HasFullStack = true,
            Stack = $"   at {Faker.Random.String2(8)}.{Faker.Random.String2(8)}() in /src/{Faker.Random.String2(8)}.cs:line {Faker.Random.Int(1, 500)}"
        };
        details.ParsedStack.Add(CreateStackFrame());
        return details;
    }

    public static StackFrame CreateStackFrame()
    {
        return new StackFrame
        {
            Level = 0,
            Method = $"{Faker.Random.String2(8)}.{Faker.Random.String2(8)}",
            Assembly = $"{Faker.Random.String2(8)}, Version=1.0.0.0",
            FileName = $"/src/{Faker.Random.String2(8)}.cs",
            Line = Faker.Random.Int(1, 500)
        };
    }

    public static AppInsightsTelemetryEnvelope CreateExceptionEnvelope()
    {
        return new AppInsightsTelemetryEnvelope
        {
            Name = "Microsoft.ApplicationInsights.Exception",
            Time = DateTime.UtcNow.ToString("O"),
            InstrumentationKey = Faker.Random.Guid().ToString(),
            Tags = CreateTags(),
            Data = new AppInsightsData
            {
                BaseType = "ExceptionData",
                BaseData = new AppInsightsBaseData
                {
                    ProblemId = $"NullRef_{Faker.Random.String2(8)}",
                    SeverityLevel = 3,
                    Exceptions = new List<AppInsightsExceptionDetails>
                    {
                        new()
                        {
                            Id = 1,
                            TypeName = "System.NullReferenceException",
                            Message = "Object reference not set to an instance of an object",
                            HasFullStack = true,
                            Stack = "   at Test.Method() in /src/Test.cs:line 42"
                        }
                    }
                }
            }
        };
    }

    #endregion

    #region Trace Telemetry

    public static FlatTrace CreateFlatTrace()
    {
        return new FlatTrace
        {
            Trace = CreateTraceTelemetry(),
            Envelope = CreateTelemetryEnvelope()
        };
    }

    public static TraceTelemetry CreateTraceTelemetry()
    {
        var trace = new TraceTelemetry
        {
            Message = Faker.Lorem.Sentence(),
            SeverityLevel = Faker.PickRandom<SeverityLevel>()
        };
        AddProperties(trace.Properties);
        return trace;
    }

    public static AppInsightsTelemetryEnvelope CreateTraceEnvelope()
    {
        return new AppInsightsTelemetryEnvelope
        {
            Name = "Microsoft.ApplicationInsights.Message",
            Time = DateTime.UtcNow.ToString("O"),
            InstrumentationKey = Faker.Random.Guid().ToString(),
            Tags = CreateTags(),
            Data = new AppInsightsData
            {
                BaseType = "MessageData",
                BaseData = new AppInsightsBaseData
                {
                    Message = Faker.Lorem.Sentence(),
                    SeverityLevel = 1
                }
            }
        };
    }

    #endregion

    #region Event Telemetry

    public static FlatEvent CreateFlatEvent()
    {
        return new FlatEvent
        {
            Event = CreateEventTelemetry(),
            Envelope = CreateTelemetryEnvelope()
        };
    }

    public static EventTelemetry CreateEventTelemetry()
    {
        var evt = new EventTelemetry
        {
            Name = Faker.PickRandom("UserSignedUp", "OrderPlaced", "ButtonClicked", "PageViewed")
        };
        AddProperties(evt.Properties);
        AddMeasurements(evt.Measurements);
        return evt;
    }

    public static AppInsightsTelemetryEnvelope CreateEventEnvelope()
    {
        return new AppInsightsTelemetryEnvelope
        {
            Name = "Microsoft.ApplicationInsights.Event",
            Time = DateTime.UtcNow.ToString("O"),
            InstrumentationKey = Faker.Random.Guid().ToString(),
            Tags = CreateTags(),
            Data = new AppInsightsData
            {
                BaseType = "EventData",
                BaseData = new AppInsightsBaseData
                {
                    Name = "UserSignedUp",
                    Properties = CreatePropertiesDict(),
                    Measurements = CreateMeasurementsDict()
                }
            }
        };
    }

    #endregion

    #region Metric Telemetry

    public static FlatMetric CreateFlatMetric()
    {
        return new FlatMetric
        {
            Metric = CreateMetricTelemetry(),
            Envelope = CreateTelemetryEnvelope()
        };
    }

    public static MetricTelemetry CreateMetricTelemetry()
    {
        var metric = new MetricTelemetry
        {
            Name = Faker.PickRandom("RequestsPerSecond", "MemoryUsage", "CpuUsage", "ResponseTime"),
            MetricNamespace = Faker.Commerce.Department(),
            Value = Faker.Random.Double(0, 1000),
            Count = Faker.Random.Int(1, 1000),
            Min = Faker.Random.Double(0, 100),
            Max = Faker.Random.Double(100, 1000),
            StdDev = Faker.Random.Double(0, 50)
        };
        AddProperties(metric.Properties);
        return metric;
    }

    public static AppInsightsTelemetryEnvelope CreateMetricEnvelope()
    {
        return new AppInsightsTelemetryEnvelope
        {
            Name = "Microsoft.ApplicationInsights.Metric",
            Time = DateTime.UtcNow.ToString("O"),
            InstrumentationKey = Faker.Random.Guid().ToString(),
            Tags = CreateTags(),
            Data = new AppInsightsData
            {
                BaseType = "MetricData",
                BaseData = new AppInsightsBaseData
                {
                    Metrics = new List<AppInsightsMetricData>
                    {
                        new()
                        {
                            Name = "RequestsPerSecond",
                            Value = 42.5,
                            Count = 100,
                            Min = 1.0,
                            Max = 100.0,
                            StdDev = 15.5
                        }
                    }
                }
            }
        };
    }

    #endregion

    #region PageView Telemetry

    public static FlatPageView CreateFlatPageView()
    {
        return new FlatPageView
        {
            PageView = CreatePageViewTelemetry(),
            Envelope = CreateTelemetryEnvelope()
        };
    }

    public static PageViewTelemetry CreatePageViewTelemetry()
    {
        var pageView = new PageViewTelemetry
        {
            Id = Faker.Random.Guid().ToString(),
            Name = Faker.PickRandom("Home Page", "Dashboard", "Settings", "Profile"),
            Url = Faker.Internet.Url(),
            Duration = $"00:00:0{Faker.Random.Int(1, 9)}.{Faker.Random.Int(100, 999)}",
            ReferrerUri = Faker.Internet.Url()
        };
        AddProperties(pageView.Properties);
        AddMeasurements(pageView.Measurements);
        return pageView;
    }

    public static AppInsightsTelemetryEnvelope CreatePageViewEnvelope()
    {
        return new AppInsightsTelemetryEnvelope
        {
            Name = "Microsoft.ApplicationInsights.PageView",
            Time = DateTime.UtcNow.ToString("O"),
            InstrumentationKey = Faker.Random.Guid().ToString(),
            Tags = CreateTags(),
            Data = new AppInsightsData
            {
                BaseType = "PageViewData",
                BaseData = new AppInsightsBaseData
                {
                    Id = Faker.Random.Guid().ToString(),
                    Name = "Home Page",
                    Url = "https://example.com/",
                    Duration = "00:00:02.500",
                    ReferrerUri = "https://google.com"
                }
            }
        };
    }

    #endregion

    #region Availability Telemetry

    public static FlatAvailability CreateFlatAvailability()
    {
        return new FlatAvailability
        {
            Availability = CreateAvailabilityTelemetry(),
            Envelope = CreateTelemetryEnvelope()
        };
    }

    public static AvailabilityTelemetry CreateAvailabilityTelemetry()
    {
        var success = Faker.Random.Bool(0.95f);
        var availability = new AvailabilityTelemetry
        {
            Id = Faker.Random.Guid().ToString(),
            Name = Faker.PickRandom("Health Check - Production", "API Endpoint Test", "Database Connectivity"),
            Duration = $"00:00:0{Faker.Random.Int(0, 5)}.{Faker.Random.Int(100, 999)}",
            Success = success,
            RunLocation = Faker.PickRandom("West US", "East US", "West Europe", "East Asia"),
            Message = success ? "Passed" : "Connection timeout"
        };
        AddProperties(availability.Properties);
        AddMeasurements(availability.Measurements);
        return availability;
    }

    public static AppInsightsTelemetryEnvelope CreateAvailabilityEnvelope()
    {
        return new AppInsightsTelemetryEnvelope
        {
            Name = "Microsoft.ApplicationInsights.Availability",
            Time = DateTime.UtcNow.ToString("O"),
            InstrumentationKey = Faker.Random.Guid().ToString(),
            Tags = CreateTags(),
            Data = new AppInsightsData
            {
                BaseType = "AvailabilityData",
                BaseData = new AppInsightsBaseData
                {
                    Id = Faker.Random.Guid().ToString(),
                    Name = "Health Check",
                    Duration = "00:00:01.500",
                    Success = true,
                    RunLocation = "West US",
                    Message = "Passed"
                }
            }
        };
    }

    #endregion

    #region Helper Methods

    private static void AddProperties(Google.Protobuf.Collections.MapField<string, string> properties, int count = 2)
    {
        for (int i = 0; i < count; i++)
        {
            properties[$"prop{i}"] = Faker.Random.String2(8);
        }
    }

    private static void AddMeasurements(Google.Protobuf.Collections.MapField<string, double> measurements, int count = 2)
    {
        for (int i = 0; i < count; i++)
        {
            measurements[$"metric{i}"] = Faker.Random.Double(0, 100);
        }
    }

    private static Dictionary<string, string> CreateTags()
    {
        return new Dictionary<string, string>
        {
            ["ai.operation.id"] = Faker.Random.Guid().ToString(),
            ["ai.operation.name"] = $"GET /api/{Faker.Random.String2(8)}",
            ["ai.cloud.roleName"] = Faker.Commerce.ProductName().Replace(" ", ""),
            ["ai.cloud.roleInstance"] = $"instance-{Faker.Random.Int(1, 10)}",
            ["ai.user.id"] = Faker.Random.Guid().ToString(),
            ["ai.session.id"] = Faker.Random.Guid().ToString()
        };
    }

    private static Dictionary<string, string> CreatePropertiesDict()
    {
        return new Dictionary<string, string>
        {
            ["customProp1"] = Faker.Random.String2(8),
            ["customProp2"] = Faker.Random.String2(8)
        };
    }

    private static Dictionary<string, double> CreateMeasurementsDict()
    {
        return new Dictionary<string, double>
        {
            ["customMetric1"] = Faker.Random.Double(0, 100),
            ["customMetric2"] = Faker.Random.Double(0, 100)
        };
    }

    #endregion
}
