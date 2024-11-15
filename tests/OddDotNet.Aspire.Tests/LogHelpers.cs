using Bogus;
using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Logs.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Logs.V1;

namespace OddDotNet.Aspire.Tests;

public static class LogHelpers
{
    public static ExportLogsServiceRequest CreateExportLogsServiceRequest()
    {
        var item = new ExportLogsServiceRequest
        {
            ResourceLogs = { CreateResourceLogs() }
        };

        return item;
    }

    public static ResourceLogs CreateResourceLogs()
    {
        var faker = new Faker();
        var item = new ResourceLogs
        {
            Resource = CommonHelpers.CreateResource(),
            SchemaUrl = faker.Internet.Url(),
            ScopeLogs = { CreateScopeLogs() }
        };

        return item;
    }

    public static ScopeLogs CreateScopeLogs()
    {
        var faker = new Faker();
        var item = new ScopeLogs
        {
            Scope = CommonHelpers.CreateInstrumentationScope(),
            SchemaUrl = faker.Internet.Url(),
            LogRecords = { CreateLogRecord() }
        };

        return item;
    }

    public static LogRecord CreateLogRecord()
    {
        var faker = new Faker();
        var sevNumber = faker.PickRandom<SeverityNumber>();
        var item = new LogRecord
        {
            TimeUnixNano = faker.Random.ULong(),
            ObservedTimeUnixNano = faker.Random.ULong(),
            SeverityNumber = sevNumber,
            SeverityText = sevNumber.ToString(),
            Body = new AnyValue
            {
                StringValue = faker.Random.String2(8)
            },
            Attributes = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            DroppedAttributesCount = faker.Random.UInt(),
            Flags = faker.Random.UInt(),
            SpanId = ByteString.CopyFrom(faker.Random.Bytes(8)),
            TraceId = ByteString.CopyFrom(faker.Random.Bytes(16))
        };

        return item;
    }
}