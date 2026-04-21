using System.Net;
using System.Text;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryGetBackCompatTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;
    public QueryGetBackCompatTests(AspireFixture fixture) { _fixture = fixture; }

    [Fact]
    public async Task PostSpans_StillReturns200()
    {
        var body = "{\"take\":{\"takeFirst\":{}},\"duration\":{\"milliseconds\":50}}";
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        var resp = await _fixture.HttpClient.PostAsync("/query/v1/spans", content);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task DeleteSpans_StillReturns204()
    {
        var resp = await _fixture.HttpClient.DeleteAsync("/query/v1/spans");
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task DeleteAll_StillReturns204()
    {
        var resp = await _fixture.HttpClient.DeleteAsync("/query/v1/all");
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }
}
