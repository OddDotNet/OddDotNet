using Grpc.Core;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OtelAnyValue = OpenTelemetry.Proto.Common.V1.AnyValue;

namespace OddDotNet.Services;

public class TraceService : OpenTelemetry.Proto.Collector.Trace.V1.TraceService.TraceServiceBase
{
    private readonly ISignalList<Span> _spans;

    public TraceService(ISignalList<Span> spans)
    {
        _spans = spans;
    }

    public override Task<ExportTraceServiceResponse> Export(ExportTraceServiceRequest request, ServerCallContext context)
    {
        foreach (var resource in request.ResourceSpans)
        {
            foreach (var instrumentationScope in resource.ScopeSpans)
            {
                foreach (var span in instrumentationScope.Spans)
                {
                    Span spanToAdd = new Span()
                    {
                        InstrumentationScope = new InstrumentationScope()
                        {
                            Name = instrumentationScope.Scope.Name,
                            Resource = new Resource()
                            {
                                SchemaUrl = resource.SchemaUrl
                            },
                            Version = instrumentationScope.Scope.Version,
                            SchemaUrl = instrumentationScope.SchemaUrl
                        },
                        Name = span.Name,
                        Flags = span.Flags,
                        Kind = (SpanKind)span.Kind, // TODO: make sure this actually works, not sure on syntax
                        Status = new SpanStatus() // TODO: determine if there's a better way we want to handle span status being null
                        {
                            Code = span.Status is not null ? (SpanStatusCode)span.Status.Code : SpanStatusCode.Unset,
                            Message = span.Status is not null ? span.Status.Message : string.Empty
                        },
                        SpanId = span.SpanId,
                        TraceId = span.TraceId,
                        TraceState = span.TraceState,
                        ParentSpanId = span.ParentSpanId,
                        EndTimeUnixNano = span.EndTimeUnixNano,
                        StartTimeUnixNano = span.StartTimeUnixNano
                    };
                    
                    foreach (var kvp in span.Attributes)
                    {
                        AnyValue value = GetAnyValue(kvp.Value);
                        spanToAdd.Attributes.Add(kvp.Key, value);
                    }
                    
                    foreach (var spanEvent in span.Events)
                    {
                        SpanEvent spanEventToAdd = new SpanEvent()
                        {
                            Name = spanEvent.Name,
                            TimeUnixNano = spanEvent.TimeUnixNano
                        };
                    
                        foreach (var kvp in spanEvent.Attributes)
                        {
                            AnyValue value = GetAnyValue(kvp.Value);
                            spanEventToAdd.Attributes.Add(kvp.Key, value);
                        }
                        
                        spanToAdd.Events.Add(spanEventToAdd);
                    }
                    
                    foreach (var link in span.Links)
                    {
                        SpanLink linkToAdd = new SpanLink()
                        {
                            SpanId = link.SpanId,
                            Flags = link.Flags,
                            TraceId = link.TraceId,
                            TraceState = link.TraceState
                        };
                        
                        foreach (var kvp in link.Attributes)
                        {
                            AnyValue value = GetAnyValue(kvp.Value);
                            linkToAdd.Attributes.Add(kvp.Key, value);
                        }
                        
                        spanToAdd.Links.Add(linkToAdd);
                    }
                    
                    foreach (var kvp in instrumentationScope.Scope.Attributes)
                    {
                        AnyValue value = GetAnyValue(kvp.Value);
                        spanToAdd.InstrumentationScope.Attributes.Add(kvp.Key, value);
                    }
                    
                    foreach (var kvp in resource.Resource.Attributes)
                    {
                        AnyValue value = GetAnyValue(kvp.Value);
                        spanToAdd.InstrumentationScope.Resource.Attributes.Add(kvp.Key, value);
                    }
                    
                    _spans.Add(spanToAdd);
                }
            }
        }

        return Task.FromResult(new ExportTraceServiceResponse());
    }

    private static AnyValue GetAnyValue(OtelAnyValue otelValue) => otelValue.ValueCase switch
    {
        OtelAnyValue.ValueOneofCase.StringValue => new AnyValue() { StringValue = otelValue.StringValue },
        OtelAnyValue.ValueOneofCase.IntValue => new AnyValue() { IntValue = otelValue.IntValue },
        _ => throw new NotImplementedException("OTEL type not yet implemented")
    };
}