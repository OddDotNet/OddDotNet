using Bogus;
using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Trace.V1;
using OtelAnyValue = OpenTelemetry.Proto.Common.V1.AnyValue;
using OtelSpanEvent = OpenTelemetry.Proto.Trace.V1.Span.Types.Event;
using OtelSpanLink = OpenTelemetry.Proto.Trace.V1.Span.Types.Link;
using OtelInstrumentationScope = OpenTelemetry.Proto.Common.V1.InstrumentationScope;
using OtelResource = OpenTelemetry.Proto.Resource.V1.Resource;
using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OtelSpanKind = OpenTelemetry.Proto.Trace.V1.Span.Types.SpanKind;

namespace OddDotNet.Aspire.Tests;

// TODO naming of this class, and location in project.

public static class TestHelpers
{
    public static OtelSpan CreateSpan()
    {
        var faker = new Faker();
        var item = new OtelSpan()
        {
            Name = faker.Random.String2(8),
            Attributes = { CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            Kind = faker.PickRandom<OtelSpanKind>(),
            Status = CreateSpanStatus(),
            SpanId = ByteString.CopyFrom(faker.Random.Bytes(8)),
            TraceId = ByteString.CopyFrom(faker.Random.Bytes(16))
        };

        return item;
    }

    public static OtelSpanEvent CreateSpanEvent()
    {
        var faker = new Faker();
        var item = new OtelSpanEvent()
        {
            Name = faker.Random.String2(8),
            Attributes = { CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) }
        };

        return item;
    }

    // TODO Implement
    public static OtelSpanLink CreateSpanLink()
    {
        throw new NotImplementedException();
    }
    
    public static Status CreateSpanStatus()
    {
        var builder = new Faker<Status>()
            .RuleFor(s => s.Code, f => f.PickRandom<Status.Types.StatusCode>())
            .RuleFor(s => s.Message, (f, s) => s.Code == Status.Types.StatusCode.Error ? f.Random.String2(8) : string.Empty);
        
        var generated = builder.Generate();
        return generated;
    }
    
    public static OtelInstrumentationScope CreateInstrumentationScope()
    {
        var faker = new Faker();
        var item = new OtelInstrumentationScope()
        {
            Name = faker.Random.String2(8),
            Version = "1",
            Attributes = { CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) }
        };
        
        return item;
    }

    public static ScopeSpans CreateScopeSpans()
    {
        var faker = new Faker();
        var item = new ScopeSpans()
        {
            SchemaUrl = faker.Internet.Url(),
            Scope = CreateInstrumentationScope(),
            Spans = { CreateSpan() }
        };

        return item;
    }

    public static OtelResource CreateResource()
    {
        var faker = new Faker();
        var item = new OtelResource()
        {
            Attributes = { CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) }
        };

        return item;
    }
    
    public static ResourceSpans CreateResourceSpans()
    {
        var faker = new Faker();
        var item = new ResourceSpans()
        {
            Resource = CreateResource(),
            SchemaUrl = faker.Internet.Url(),
            ScopeSpans = { CreateScopeSpans() }
        };

        return item;
    }
    
    public static ExportTraceServiceRequest CreateExportTraceServiceRequest()
    {
        var item = new ExportTraceServiceRequest()
        {
            ResourceSpans = { CreateResourceSpans() }
        };

        return item;
    }

    public static KeyValue CreateKeyValue<TValue>(string key, TValue value)
    {
        var anyValue = value switch
        {
            _ when value is string s => new OtelAnyValue(){ StringValue = s },
            _ when value is int i => new OtelAnyValue(){ IntValue = i },
            _ when value is double d => new OtelAnyValue(){ DoubleValue = d },
            _ when value is bool b => new OtelAnyValue(){ BoolValue = b },
            _ when value is ByteString b => new OtelAnyValue(){ BytesValue = b},
            _ => throw new NotImplementedException(), // TODO Is this the right exception?
        };

        return new KeyValue() { Key = key, Value = anyValue };
    }
}