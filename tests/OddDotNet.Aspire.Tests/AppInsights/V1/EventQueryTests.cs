using System.Text;
using System.Text.Json;
using OddDotNet.Proto.AppInsights.V1.Event;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for Event telemetry query filtering.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class EventQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public EventQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task IngestEvent(AppInsightsTelemetryEnvelope envelope)
    {
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Query_WhenFilteringByName_WithEquals_ShouldReturnMatchingEvent()
    {
        // Arrange
        var uniqueName = $"test-evt-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateEventEnvelope();
        envelope.Data!.BaseData!.Name = uniqueName;
        await IngestEvent(envelope);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty { Compare = uniqueName, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiEventQueryServiceClient.QueryAsync(
            new EventQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Events);
        Assert.Equal(uniqueName, response.Events[0].Event.Name);
    }

    [Fact]
    public async Task Query_WhenFilteringByName_WithContains_ShouldReturnMatchingEvent()
    {
        // Arrange
        var marker = $"marker-{Guid.NewGuid():N}";
        var uniqueName = $"UserSignedUp_{marker}";
        var envelope = AppInsightsHelpers.CreateEventEnvelope();
        envelope.Data!.BaseData!.Name = uniqueName;
        await IngestEvent(envelope);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty { Compare = marker, CompareAs = StringCompareAsType.Contains }
            }
        };

        // Act
        var response = await _fixture.AiEventQueryServiceClient.QueryAsync(
            new EventQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Events);
        Assert.Equal(uniqueName, response.Events[0].Event.Name);
    }

    [Fact]
    public async Task Query_WhenNoMatchingEvents_ShouldReturnEmptyResult()
    {
        // Arrange - use a non-existent name
        var nonExistentName = $"non-existent-evt-{Guid.NewGuid():N}";

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty { Compare = nonExistentName, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiEventQueryServiceClient.QueryAsync(
            new EventQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Empty(response.Events);
    }
}
