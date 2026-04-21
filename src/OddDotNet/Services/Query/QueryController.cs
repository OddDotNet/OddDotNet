using Google.Protobuf;

using Microsoft.AspNetCore.Mvc;

using OddDotNet.Services.Query.Shorthand;

namespace OddDotNet.Services.Query;

[ApiController]
[Route("query/v1")]
public class QueryController : ControllerBase
{
    private const string JsonContentType = "application/json";

    private readonly QueryDispatcher _dispatcher;
    private readonly ILogger<QueryController> _logger;

    public QueryController(QueryDispatcher dispatcher, ILogger<QueryController> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    [HttpPost("{*signalPath}")]
    public async Task<IActionResult> Post(string signalPath)
    {
        if (!_dispatcher.TryGet(signalPath, out var handler))
        {
            return NotFound(new { error = $"unknown signal '{signalPath}'" });
        }

        var contentType = Request.ContentType ?? string.Empty;
        if (!contentType.StartsWith(JsonContentType, StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }

        try
        {
            var json = await handler.QueryAsJsonAsync(body, HttpContext.RequestAborted);
            return Content(json, JsonContentType);
        }
        catch (InvalidJsonException ex)
        {
            _logger.LogWarning(ex, "Malformed query JSON for {SignalPath}", signalPath);
            return BadRequest(new { error = "malformed JSON body" });
        }
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogWarning(ex, "Query body failed proto validation for {SignalPath}", signalPath);
            return BadRequest(new { error = "invalid filter: " + ex.Message });
        }
    }

    [HttpGet("{*signalPath}")]
    public async Task<IActionResult> Get(string signalPath)
    {
        if (!_dispatcher.TryGet(signalPath, out var handler))
        {
            return NotFound(new { error = $"unknown signal '{signalPath}'" });
        }

        if (!handler.SupportsGetShorthand)
        {
            return StatusCode(StatusCodes.Status405MethodNotAllowed,
                new { error = $"signal '{signalPath}' does not support GET shorthand" });
        }

        try
        {
            var json = await handler.QueryAsJsonFromQueryStringAsync(Request.Query, HttpContext.RequestAborted);
            return Content(json, JsonContentType);
        }
        catch (ShorthandParseException ex)
        {
            _logger.LogWarning(ex, "Shorthand parse error for {SignalPath}", signalPath);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{*signalPath}")]
    public IActionResult Delete(string signalPath)
    {
        if (string.Equals(signalPath, "all", StringComparison.OrdinalIgnoreCase))
        {
            _dispatcher.ResetAll();
            return NoContent();
        }

        if (!_dispatcher.TryGet(signalPath, out var handler))
        {
            return NotFound(new { error = $"unknown signal '{signalPath}'" });
        }

        handler.Reset();
        return NoContent();
    }
}
