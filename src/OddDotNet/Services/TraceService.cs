using Grpc.Core;
using OpenTelemetry.Proto.Collector.Trace.V1;

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
                        Scope = new Scope()
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
                        Kind = (SpanKind)span.Kind, // TODO make sure this actually works, not sure on syntax
                        Status = new SpanStatus()
                        {
                            Code = (SpanStatusCode)span.Status.Code,
                            Message = span.Status.Message
                        },
                        SpanId = span.SpanId.ToByteArray(),
                        TraceId = span.TraceId.ToByteArray(),
                        TraceState = span.TraceState,
                        ParentSpanId = span.ParentSpanId.ToByteArray(),
                        EndTimeUnixNano = span.EndTimeUnixNano,
                        StartTimeUnixNano = span.StartTimeUnixNano
                    };

                    foreach (var kvp in span.Attributes)
                    {
                        spanToAdd.Attributes.Add(kvp.Key, kvp.Value);
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
                            spanEventToAdd.Attributes.Add(kvp.Key, kvp.Value);
                        }
                        
                        spanToAdd.Events.Add(spanEventToAdd);
                    }
                    
                    foreach (var link in span.Links)
                    {
                        SpanLink linkToAdd = new SpanLink()
                        {
                            SpanId = link.SpanId.ToByteArray(),
                            Flags = link.Flags,
                            TraceId = link.TraceId.ToByteArray(),
                            TraceState = link.TraceState
                        };
                        
                        foreach (var kvp in link.Attributes)
                        {
                            linkToAdd.Attributes.Add(kvp.Key, kvp.Value);
                        }
                        
                        spanToAdd.Links.Add(linkToAdd);
                    }
                    
                    foreach (var kvp in instrumentationScope.Scope.Attributes)
                    {
                        spanToAdd.Scope.Attributes.Add(kvp.Key, kvp.Value);
                    }

                    foreach (var kvp in resource.Resource.Attributes)
                    {
                        spanToAdd.Scope.Resource.Attributes.Add(kvp.Key, kvp.Value);
                    }
                }
            }
        }
        return base.Export(request, context);
    }
}