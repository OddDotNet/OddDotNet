using Microsoft.AspNetCore.Http;

namespace OddDotNet.Services.Query;

public interface ISignalQueryHandler
{
    string SignalPath { get; }

    bool SupportsGetShorthand { get; }

    Task<string> QueryAsJsonAsync(string jsonBody, CancellationToken cancellationToken);

    Task<string> QueryAsJsonFromQueryStringAsync(IQueryCollection query, CancellationToken cancellationToken);

    void Reset();
}
