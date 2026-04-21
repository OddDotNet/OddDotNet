using System.Net;
using System.Text;
using System.Text.Json;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryErrorsTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public QueryErrorsTests(AspireFixture fixture) { _fixture = fixture; }

    [Fact]
    public async Task DeleteUnknownSignal_ShouldReturn404()
    {
        var response = await _fixture.HttpClient.DeleteAsync("/query/v1/widgets");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostUnknownSignal_ShouldReturn404()
    {
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/query/v1/widgets", content);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostSpans_WithExplicitShortDurationOnEmptyFilter_ShouldReturn200WithItemsArray()
    {
        // {} would long-poll on the server-default timeout; pin it to a short window.
        var body = "{\"take\":{\"takeAll\":{}},\"duration\":{\"milliseconds\":50}}";
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/query/v1/spans", content);

        response.EnsureSuccessStatusCode();
        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(JsonValueKind.Array, root.GetProperty("items").ValueKind);
    }
}
