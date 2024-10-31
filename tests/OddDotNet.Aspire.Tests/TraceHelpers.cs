using Bogus;
using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Trace.V1;
using OtelSpanEvent = OpenTelemetry.Proto.Trace.V1.Span.Types.Event;
using OtelSpanLink = OpenTelemetry.Proto.Trace.V1.Span.Types.Link;
using OtelSpan = OpenTelemetry.Proto.Trace.V1.Span;
using OtelSpanKind = OpenTelemetry.Proto.Trace.V1.Span.Types.SpanKind;

namespace OddDotNet.Aspire.Tests;


public static class TraceHelpers
{
    public static OtelSpan CreateSpan()
    {
        var faker = new Faker();
        var item = new OtelSpan()
        {
            Name = faker.Random.String2(8),
            Attributes = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            Kind = faker.PickRandom<OtelSpanKind>(),
            Status = CreateSpanStatus(),
            SpanId = ByteString.CopyFrom(faker.Random.Bytes(8)),
            TraceId = ByteString.CopyFrom(faker.Random.Bytes(16))
        };
        item.Events.Add(CreateSpanEvent());
        item.Links.Add(CreateSpanLink());

        return item;
    }

    public static OtelSpanEvent CreateSpanEvent()
    {
        var faker = new Faker();
        var item = new OtelSpanEvent()
        {
            Name = faker.Random.String2(8),
            Attributes = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) }
        };

        return item;
    }

    public static OtelSpanLink CreateSpanLink()
    {
        var faker = new Faker();
        var item = new OtelSpanLink()
        {
            TraceId = ByteString.CopyFrom(faker.Random.Bytes(16)),
            SpanId = ByteString.CopyFrom(faker.Random.Bytes(8)),
            TraceState = faker.Random.String2(8),
            Attributes = { CommonHelpers.CreateKeyValue(faker.Random.String2(8), faker.Random.String2(8)) },
            Flags = faker.Random.UInt()
        };

        return item;
    }
    
    public static Status CreateSpanStatus()
    {
        var builder = new Faker<Status>()
            .RuleFor(s => s.Code, f => f.PickRandom<Status.Types.StatusCode>())
            .RuleFor(s => s.Message, (f, s) => s.Code == Status.Types.StatusCode.Error ? f.Random.String2(8) : string.Empty);
        
        var generated = builder.Generate();
        return generated;
    }
    
    

    public static ScopeSpans CreateScopeSpans()
    {
        var faker = new Faker();
        var item = new ScopeSpans()
        {
            SchemaUrl = faker.Internet.Url(),
            Scope = CommonHelpers.CreateInstrumentationScope(),
            Spans = { CreateSpan() }
        };

        return item;
    }

    
    
    public static ResourceSpans CreateResourceSpans()
    {
        var faker = new Faker();
        var item = new ResourceSpans()
        {
            Resource = CommonHelpers.CreateResource(),
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

    
}