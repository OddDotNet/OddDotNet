using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

public class AppInsightsBreezeEndpointTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public AppInsightsBreezeEndpointTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Reset_ShouldClearAllSignalTypes()
    {
        // Arrange - ingest one of each type
        var envelopes = new[]
        {
            AppInsightsHelpers.CreateRequestEnvelope(),
            AppInsightsHelpers.CreateDependencyEnvelope(),
            AppInsightsHelpers.CreateExceptionEnvelope(),
            AppInsightsHelpers.CreateTraceEnvelope(),
            AppInsightsHelpers.CreateEventEnvelope(),
            AppInsightsHelpers.CreateMetricEnvelope(),
            AppInsightsHelpers.CreatePageViewEnvelope(),
            AppInsightsHelpers.CreateAvailabilityEnvelope()
        };

        foreach (var envelope in envelopes)
        {
            var json = JsonSerializer.Serialize(envelope);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _fixture.HttpClient.PostAsync("/v2/track", content);
        }

        // Act
        var resetResponse = await _fixture.HttpClient.DeleteAsync("/appinsights/reset");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        var summary = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights");
        Assert.Equal(0, summary.GetProperty("requests").GetInt32());
        Assert.Equal(0, summary.GetProperty("dependencies").GetInt32());
        Assert.Equal(0, summary.GetProperty("exceptions").GetInt32());
        Assert.Equal(0, summary.GetProperty("traces").GetInt32());
        Assert.Equal(0, summary.GetProperty("events").GetInt32());
        Assert.Equal(0, summary.GetProperty("metrics").GetInt32());
        Assert.Equal(0, summary.GetProperty("pageViews").GetInt32());
        Assert.Equal(0, summary.GetProperty("availability").GetInt32());
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyAfterReset()
    {
        // Arrange - ingest and then reset
        var json = JsonSerializer.Serialize(AppInsightsHelpers.CreateRequestEnvelope());
        await _fixture.HttpClient.PostAsync("/v2/track", new StringContent(json, Encoding.UTF8, "application/json"));
        await _fixture.HttpClient.DeleteAsync("/appinsights/reset");

        // Assert all array endpoints return empty
        var requests = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/requests");
        var dependencies = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/dependencies");
        var exceptions = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/exceptions");
        var traces = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/traces");
        var events = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/events");
        var metrics = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/metrics");
        var pageViews = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/pageviews");
        var availability = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/availability");

        Assert.Equal(0, requests.GetArrayLength());
        Assert.Equal(0, dependencies.GetArrayLength());
        Assert.Equal(0, exceptions.GetArrayLength());
        Assert.Equal(0, traces.GetArrayLength());
        Assert.Equal(0, events.GetArrayLength());
        Assert.Equal(0, metrics.GetArrayLength());
        Assert.Equal(0, pageViews.GetArrayLength());
        Assert.Equal(0, availability.GetArrayLength());
    }

    [Fact]
    public async Task SummaryCount_ShouldMatchGetAllCount()
    {
        // Arrange - reset then ingest known quantities
        await _fixture.HttpClient.DeleteAsync("/appinsights/reset");

        var envelopes = new[]
        {
            AppInsightsHelpers.CreateRequestEnvelope(),
            AppInsightsHelpers.CreateRequestEnvelope(),
            AppInsightsHelpers.CreateDependencyEnvelope(),
            AppInsightsHelpers.CreateExceptionEnvelope(),
            AppInsightsHelpers.CreateTraceEnvelope(),
            AppInsightsHelpers.CreateEventEnvelope(),
            AppInsightsHelpers.CreateMetricEnvelope(),
            AppInsightsHelpers.CreatePageViewEnvelope(),
            AppInsightsHelpers.CreateAvailabilityEnvelope()
        };

        foreach (var envelope in envelopes)
        {
            var json = JsonSerializer.Serialize(envelope);
            await _fixture.HttpClient.PostAsync("/v2/track", new StringContent(json, Encoding.UTF8, "application/json"));
        }

        // Act
        var summary = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights");
        var requests = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/requests");
        var dependencies = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/dependencies");
        var exceptions = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/exceptions");
        var traces = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/traces");
        var events = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/events");
        var metrics = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/metrics");
        var pageViews = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/pageviews");
        var availability = await _fixture.HttpClient.GetFromJsonAsync<JsonElement>("/appinsights/availability");

        // Assert - summary counts must match array lengths
        Assert.Equal(summary.GetProperty("requests").GetInt32(), requests.GetArrayLength());
        Assert.Equal(summary.GetProperty("dependencies").GetInt32(), dependencies.GetArrayLength());
        Assert.Equal(summary.GetProperty("exceptions").GetInt32(), exceptions.GetArrayLength());
        Assert.Equal(summary.GetProperty("traces").GetInt32(), traces.GetArrayLength());
        Assert.Equal(summary.GetProperty("events").GetInt32(), events.GetArrayLength());
        Assert.Equal(summary.GetProperty("metrics").GetInt32(), metrics.GetArrayLength());
        Assert.Equal(summary.GetProperty("pageViews").GetInt32(), pageViews.GetArrayLength());
        Assert.Equal(summary.GetProperty("availability").GetInt32(), availability.GetArrayLength());

        // Also verify the known quantities
        Assert.Equal(2, requests.GetArrayLength());
        Assert.Equal(1, dependencies.GetArrayLength());
        Assert.Equal(1, exceptions.GetArrayLength());
        Assert.Equal(1, traces.GetArrayLength());
        Assert.Equal(1, events.GetArrayLength());
        Assert.Equal(1, metrics.GetArrayLength());
        Assert.Equal(1, pageViews.GetArrayLength());
        Assert.Equal(1, availability.GetArrayLength());
    }
}
