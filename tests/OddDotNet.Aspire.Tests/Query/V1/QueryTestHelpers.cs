using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Google.Protobuf;

namespace OddDotNet.Aspire.Tests.Query.V1;

internal static class QueryTestHelpers
{
    public static StringContent JsonContent(IMessage request)
    {
        var json = JsonFormatter.Default.Format(request);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<JsonElement> PostQueryAsync(HttpClient client, string path, IMessage queryRequest)
    {
        using var content = JsonContent(queryRequest);
        var response = await client.PostAsync(path, content);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body).RootElement;
    }

    public static StringContent RawJsonContent(string json) =>
        new(json, Encoding.UTF8, "application/json");
}
