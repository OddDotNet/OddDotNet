using Bogus;
using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Metrics.V1;

namespace OddDotNet.Aspire.Tests;

public static class MetricHelpers
{
    public static Metric CreateMetric(MetricType metricType)
    {
        var faker = new Faker();
        var item = new Metric
        {
            Name = faker.Random.String2(8),
            Description = faker.Random.String2(8),
            Unit = faker.Random.String2(8),
            Metadata = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) }
        };

        switch (metricType)
        {
            case MetricType.Gauge:
                item.Gauge = CreateGauge();
                break;
            case MetricType.Sum:
                item.Sum = CreateSum();
                break;
            case MetricType.Histogram:
                item.Histogram = CreateHistogram();
                break;
            case MetricType.ExponentialHistogram:
                item.ExponentialHistogram = CreateExponentialHistogram();
                break;
            case MetricType.Summary:
                item.Summary = CreateSummary();
                break;
        }

        return item;
    }

    public static Gauge CreateGauge()
    {
        var item = new Gauge
        {
            DataPoints = { CreateNumberDataPoint() }
        };

        return item;
    }

    public static Sum CreateSum()
    {
        var faker = new Faker();
        var item = new Sum
        {
            DataPoints = { CreateNumberDataPoint() },
            AggregationTemporality = faker.PickRandom<AggregationTemporality>(),
            IsMonotonic = true
        };

        return item;
    }

    public static Histogram CreateHistogram()
    {
        var faker = new Faker();
        var item = new Histogram
        {
            DataPoints = { CreateHistogramDataPoint() },
            AggregationTemporality = faker.PickRandom<AggregationTemporality>()
        };

        return item;
    }
    
    public static ExponentialHistogram CreateExponentialHistogram()
    {
        var faker = new Faker();
        var item = new ExponentialHistogram
        {
            DataPoints = { CreateExponentialHistogramDataPoint() },
            AggregationTemporality = faker.PickRandom<AggregationTemporality>(),
        };

        return item;
    }

    public static Summary CreateSummary()
    {
        var item = new Summary
        {
            DataPoints = { CreateSummaryDataPoint() }
        };

        return item;
    }

    public static SummaryDataPoint CreateSummaryDataPoint()
    {
        var faker = new Faker();
        var item = new SummaryDataPoint
        {
            Attributes = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            StartTimeUnixNano = faker.Random.ULong(),
            TimeUnixNano = faker.Random.ULong(),
            Count = faker.Random.ULong(),
            Sum = faker.Random.Double(),
            QuantileValues = { CreateValueAtQuantile() },
            Flags = faker.Random.UInt()
        };

        return item;
    }

    public static SummaryDataPoint.Types.ValueAtQuantile CreateValueAtQuantile()
    {
        var faker = new Faker();
        var item = new SummaryDataPoint.Types.ValueAtQuantile
        {
            Quantile = faker.Random.Double(),
            Value = faker.Random.Double()
        };

        return item;
    }

    public static HistogramDataPoint CreateHistogramDataPoint()
    {
        var faker = new Faker();
        var item = new HistogramDataPoint
        {
            Attributes = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            BucketCounts = { faker.Random.ULong() },
            Count = faker.Random.ULong(),
            Exemplars = { CreateExemplar() },
            ExplicitBounds = { faker.Random.Double() },
            Flags = faker.Random.UInt(),
            Max = faker.Random.Double(),
            Min = faker.Random.Double(),
            StartTimeUnixNano = faker.Random.ULong(),
            Sum = faker.Random.Double(),
            TimeUnixNano = faker.Random.ULong(),
        };
        
        return item;
    }
    
    public static ExponentialHistogramDataPoint CreateExponentialHistogramDataPoint()
    {
        var faker = new Faker();
        var item = new ExponentialHistogramDataPoint
        {
            Attributes = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            Count = faker.Random.ULong(),
            Exemplars = { CreateExemplar() },
            Flags = faker.Random.UInt(),
            Max = faker.Random.Double(),
            Min = faker.Random.Double(),
            StartTimeUnixNano = faker.Random.ULong(),
            Sum = faker.Random.Double(),
            TimeUnixNano = faker.Random.ULong(),
            Scale = faker.Random.Int(),
            ZeroCount = faker.Random.ULong(),
            Positive = CreateExponentialHistogramDataPointBucket(),
            Negative = CreateExponentialHistogramDataPointBucket(),
            ZeroThreshold = faker.Random.Double()
        };
        
        return item;
    }

    public static ExponentialHistogramDataPoint.Types.Buckets CreateExponentialHistogramDataPointBucket()
    {
        var faker = new Faker();
        var item = new ExponentialHistogramDataPoint.Types.Buckets
        {
            Offset = faker.Random.Int(),
            BucketCounts = { faker.Random.ULong() }
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
            Metrics = { CreateMetric(MetricType.Gauge) }
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

public enum MetricType
{
    Gauge,
    Sum,
    Histogram,
    ExponentialHistogram,
    Summary
}