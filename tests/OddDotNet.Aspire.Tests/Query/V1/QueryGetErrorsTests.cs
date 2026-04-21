using System.Net;

namespace OddDotNet.Aspire.Tests.Query.V1;

public class QueryGetErrorsTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;
    public QueryGetErrorsTests(AspireFixture fixture) { _fixture = fixture; }

    [Theory]
    [InlineData("spans", "favorite_color=blue")]
    [InlineData("metrics", "not_a_field=x")]
    [InlineData("logs", "bogus=1")]
    [InlineData("appinsights/requests", "not_a_field=x")]
    [InlineData("appinsights/events", "bogus=1")]
    public async Task Get_WithUnknownField_Returns400(string signalPath, string query)
    {
        var resp = await _fixture.HttpClient.GetAsync($"/query/v1/{signalPath}?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Get_UnknownSignalPath_Returns404()
    {
        var resp = await _fixture.HttpClient.GetAsync("/query/v1/widgets?name=x");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Get_WithOnlyWaitMs_ReturnsEmptyArray()
    {
        var resp = await _fixture.HttpClient.GetAsync("/query/v1/spans?wait_ms=50");
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetMetrics_WithAttrParam_Returns400()
    {
        var resp = await _fixture.HttpClient.GetAsync("/query/v1/metrics?attr.env=prod");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }
}
