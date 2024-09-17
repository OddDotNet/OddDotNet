using Bogus;
using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Trace.V1;
using OtelAnyValue = OpenTelemetry.Proto.Common.V1.AnyValue;
using OtelSpanEvent = OpenTelemetry.Proto.Trace.V1.Span.Types.Event;
using OtelSpanLink = OpenTelemetry.Proto.Trace.V1.Span.Types.Link;

namespace OddDotNet.Aspire.Tests;

public static class TestHelpers
{
    public static OpenTelemetry.Proto.Trace.V1.Span CreateSpan()
    {
        var spanBuilder = new Faker<OpenTelemetry.Proto.Trace.V1.Span>()
            .RuleFor(s => s.Name, f => f.Random.String(8))
            .RuleFor(s => s.Attributes, f => [CreateKeyValue(f.Random.String(8), f.Random.String(8))])
            // .RuleFor(s => s.Events, f => [CreateSpanEvent()])
            .RuleFor(s => s.Kind, f => f.PickRandom<OpenTelemetry.Proto.Trace.V1.Span.Types.SpanKind>())
            // .RuleFor(s => s.Links, f => [CreateSpanLink()])
            .RuleFor(s => s.Status, f => CreateSpanStatus())
            .RuleFor(s => s.SpanId, f => ByteString.CopyFrom(f.Random.Bytes(8)))
            .RuleFor(s => s.TraceId, f => ByteString.CopyFrom(f.Random.Bytes(16)))
            .RuleForType(typeof(uint), f => 0); // Set all uint to 0

        return spanBuilder.Generate();
    }

    public static OtelSpanEvent CreateSpanEvent()
    {
        var spanEventBuilder = new Faker<OtelSpanEvent>()
            .RuleFor(s => s.Name, f => f.Random.String(8))
            .RuleFor(s => s.Attributes, f => [CreateKeyValue(f.Random.String(8), f.Random.String(8))]);

        return spanEventBuilder.Generate();
    }

    // TODO Implement
    public static OtelSpanLink CreateSpanLink()
    {
        throw new NotImplementedException();
    }
    
    public static Status CreateSpanStatus()
    {
        var status = new Faker<Status>()
            .RuleFor(s => s.Code, f => f.PickRandom<Status.Types.StatusCode>())
            .RuleFor(s => s.Message, (f, s) => s.Code == Status.Types.StatusCode.Error ? f.Random.String(8) : null);
        
        return status.Generate();
    }
    
    // TODO Implement
    public static InstrumentationScope CreateInstrumentationScope()
    {
        throw new NotImplementedException();
    }

    // TODO Implement
    public static ScopeSpans CreateScopeSpans()
    {
        throw new NotImplementedException();
    }

    // TODO Implement
    public static OpenTelemetry.Proto.Resource.V1.Resource CreateResource()
    {
        throw new NotImplementedException();
    }

    // TODO Implement
    public static ResourceSpans CreateResourceSpans()
    {
        throw new NotImplementedException();
    }

    // TODO Implement
    public static ExportTraceServiceRequest CreateExportTraceServiceRequest()
    {
        throw new NotImplementedException();
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