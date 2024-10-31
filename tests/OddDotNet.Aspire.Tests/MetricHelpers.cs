using Bogus;
using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Aspire.Tests;

public static class MetricHelpers
{
    public static Metric CreateMetric()
    {
        var faker = new Faker();
        var item = new Metric
        {
            Name = faker.Random.String2(8),
            Description = faker.Random.String2(8),
            Unit = faker.Random.String2(8),
            Metadata = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            Gauge = CreateGauge()
        };

        return item;
    }

    public static Gauge CreateGauge()
    {
        var faker = new Faker();
        var item = new Gauge
        {
            DataPoints = { CreateNumberDataPoint() }
        };

        return item;
    }

    public static NumberDataPoint CreateNumberDataPoint()
    {
        var faker = new Faker();
        var item = new NumberDataPoint
        {
            Attributes = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            StartTimeUnixNano = faker.Random.ULong(),
            TimeUnixNano = faker.Random.ULong(),
            Flags = faker.Random.UInt(),
            AsDouble = faker.Random.Double(),
            Exemplars = { CreateExemplar() }
        };

        return item;
    }

    public static Exemplar CreateExemplar()
    {
        var faker = new Faker();
        var item = new Exemplar
        {
            FilteredAttributes = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            TimeUnixNano = faker.Random.ULong(),
            AsDouble = faker.Random.Double(),
            SpanId = ByteString.CopyFrom(faker.Random.Bytes(8)),
            TraceId = ByteString.CopyFrom(faker.Random.Bytes(16))
        };

        return item;
    }

    public static ScopeMetrics CreateScopeMetrics()
    {
        var faker = new Faker();
        var item = new ScopeMetrics
        {
            Scope = CommonHelpers.CreateInstrumentationScope(),
            SchemaUrl = faker.Internet.Url(),
            Metrics = { CreateMetric() }
        };

        return item;
    }

    public static ResourceMetrics CreateResourceMetrics()
    {
        var faker = new Faker();
        var item = new ResourceMetrics
        {
            Resource = CommonHelpers.CreateResource(),
            SchemaUrl = faker.Internet.Url(),
            ScopeMetrics = { CreateScopeMetrics() }
        };

        return item;
    }

    public static ExportMetricsServiceRequest CreateExportMetricsServiceRequest()
    {
        var item = new ExportMetricsServiceRequest
        {
            ResourceMetrics = { CreateResourceMetrics() }
        };

        return item;
    }
}