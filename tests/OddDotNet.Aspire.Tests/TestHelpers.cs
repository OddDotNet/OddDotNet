using Bogus;
using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Common.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet.Aspire.Tests;

public static class TestHelpers
{
    public static OpenTelemetry.Proto.Trace.V1.Span CreateSpan()
    {
        var spanBuilder = new Faker<OpenTelemetry.Proto.Trace.V1.Span>()
            .RuleFor(s => s.Name, f => f.Random.String(8))
            .RuleFor(s => s.SpanId, f => ByteString.CopyFrom(f.Random.Bytes(8)))
            .RuleFor(s => s.TraceId, f => ByteString.CopyFrom(f.Random.Bytes(16)));
        
        // TODO finish all properties

        return spanBuilder.Generate();
    }

    // TODO Implement
    public static SpanEvent CreateSpanEvent()
    {
        throw new NotImplementedException();
    }

    // TODO Implement
    public static SpanLink CreateSpanLink()
    {
        throw new NotImplementedException();
    }
    
    // TODO Implement
    public static SpanStatus CreateSpanStatus()
    {
        throw new NotImplementedException();
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

    // TODO something like this to easily generate a KeyValue
    public static KeyValue CreateKeyValue<TValue>(string key, TValue theValue)
    {
        throw new NotImplementedException();
    }
}