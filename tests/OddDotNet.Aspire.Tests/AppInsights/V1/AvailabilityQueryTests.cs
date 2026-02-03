using System.Text;
using System.Text.Json;
using OddDotNet.Proto.AppInsights.V1;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for Availability telemetry query filtering.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class AvailabilityQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public AvailabilityQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task IngestAvailability(AppInsightsTelemetryEnvelope envelope)
    {
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Query_WhenFilteringById_WithEquals_ShouldReturnMatchingAvailability()
    {
        // Arrange
        var uniqueId = $"test-avail-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateAvailabilityEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        await IngestAvailability(envelope);

        var filter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiAvailabilityQueryServiceClient.QueryAsync(
            new AvailabilityQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Availabilities);
        Assert.Equal(uniqueId, response.Availabilities[0].Availability.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringByName_WithEquals_ShouldReturnMatchingAvailability()
    {
        // Arrange
        var uniqueId = $"test-avail-{Guid.NewGuid():N}";
        var uniqueName = $"Health Check - {uniqueId}";
        var envelope = AppInsightsHelpers.CreateAvailabilityEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Name = uniqueName;
        await IngestAvailability(envelope);

        var filter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                Name = new StringProperty { Compare = uniqueName, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiAvailabilityQueryServiceClient.QueryAsync(
            new AvailabilityQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Availabilities);
        Assert.Equal(uniqueName, response.Availabilities[0].Availability.Name);
    }

    [Fact]
    public async Task Query_WhenFilteringBySuccess_ShouldReturnMatchingAvailability()
    {
        // Arrange
        var uniqueId = $"test-avail-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateAvailabilityEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Success = false;
        envelope.Data!.BaseData!.Message = "Connection timeout";
        await IngestAvailability(envelope);

        var idFilter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var successFilter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                Success = new BoolProperty { Compare = false, CompareAs = BoolCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiAvailabilityQueryServiceClient.QueryAsync(
            new AvailabilityQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, successFilter }
            });

        // Assert
        Assert.Single(response.Availabilities);
        Assert.False(response.Availabilities[0].Availability.Success);
    }

    [Fact]
    public async Task Query_WhenFilteringByRunLocation_ShouldReturnMatchingAvailability()
    {
        // Arrange
        var uniqueId = $"test-avail-{Guid.NewGuid():N}";
        var runLocation = "West US";
        var envelope = AppInsightsHelpers.CreateAvailabilityEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.RunLocation = runLocation;
        await IngestAvailability(envelope);

        var idFilter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var locationFilter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                RunLocation = new StringProperty { Compare = runLocation, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiAvailabilityQueryServiceClient.QueryAsync(
            new AvailabilityQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, locationFilter }
            });

        // Assert
        Assert.Single(response.Availabilities);
        Assert.Equal(runLocation, response.Availabilities[0].Availability.RunLocation);
    }

    [Fact]
    public async Task Query_WhenFilteringByMessage_WithContains_ShouldReturnMatchingAvailability()
    {
        // Arrange
        var marker = $"marker-{Guid.NewGuid():N}";
        var uniqueId = $"test-avail-{Guid.NewGuid():N}";
        var message = $"Connection timeout at {marker}";
        var envelope = AppInsightsHelpers.CreateAvailabilityEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Success = false;
        envelope.Data!.BaseData!.Message = message;
        await IngestAvailability(envelope);

        var idFilter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var messageFilter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                Message = new StringProperty { Compare = marker, CompareAs = StringCompareAsType.Contains }
            }
        };

        // Act
        var response = await _fixture.AiAvailabilityQueryServiceClient.QueryAsync(
            new AvailabilityQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, messageFilter }
            });

        // Assert
        Assert.Single(response.Availabilities);
        Assert.Equal(message, response.Availabilities[0].Availability.Message);
    }

    [Fact]
    public async Task Query_WhenNoMatchingAvailabilities_ShouldReturnEmptyResult()
    {
        // Arrange - use a non-existent ID
        var nonExistentId = $"non-existent-avail-{Guid.NewGuid():N}";

        var filter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                Id = new StringProperty { Compare = nonExistentId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiAvailabilityQueryServiceClient.QueryAsync(
            new AvailabilityQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Empty(response.Availabilities);
    }
}
