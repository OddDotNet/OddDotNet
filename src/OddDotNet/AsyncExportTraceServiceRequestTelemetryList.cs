using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Trace.V1;

namespace OddDotNet;

public class AsyncExportTraceServiceRequestTelemetryList : IAsyncTelemetryList<ExportTraceServiceRequest>
{
    private static readonly Channel<ExportTraceServiceRequest> _channel =
        Channel.CreateUnbounded<ExportTraceServiceRequest>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

    public async Task<bool> AnyAsync(FilterDelegate<ExportTraceServiceRequest> filter, CancellationToken cancellationToken = default)
    {
        try
        {
            await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            await Console.Error.WriteLineAsync("Operation canceled while waiting to read from the ResourceSpan channel");
        }
        
        //return _channel.Reader.ReadAllAsync(cancellationToken).ToBlockingEnumerable(cancellationToken).FirstOrDefault(filter.) != null;
        var exportTraceServiceRequests = _channel.Reader.ReadAllAsync(cancellationToken).ToBlockingEnumerable(cancellationToken);
        
        foreach (var exportTraceServiceRequest in exportTraceServiceRequests)
        {
            if (filter(exportTraceServiceRequest))
            {
                Console.WriteLine("Found a exportTraceServiceRequest that matches the filter");
                return true;
            }
        }
        
        Console.WriteLine("No exportTraceServiceRequest found that matches the filter");
        return false;
    }
    
    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ExportTraceServiceRequest> FirstAsync(FilterDelegate<ExportTraceServiceRequest> filter, CancellationToken cancellationToken = default)
    {
        try
        {
            await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            await Console.Error.WriteLineAsync("Operation canceled while waiting to read from the ResourceSpan channel");
        }
        
        var exportTraceServiceRequests = _channel.Reader.ReadAllAsync(cancellationToken).ToBlockingEnumerable(cancellationToken);
        
        foreach (var exportTraceServiceRequest in exportTraceServiceRequests)
        {
            if (filter(exportTraceServiceRequest))
            {
                Console.WriteLine("Found a exportTraceServiceRequest that matches the filter");
                return exportTraceServiceRequest;
            }
        }
        
        Console.WriteLine("No exportTraceServiceRequest found that matches the filter");
        return new ExportTraceServiceRequest();
    }

    public bool Add(ExportTraceServiceRequest element)
    {
        return _channel.Writer.TryWrite(element);
    }
}