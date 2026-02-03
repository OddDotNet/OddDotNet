using System.Text;
using System.Text.Json;
using OddDotNet.Proto.AppInsights.V1.Request;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for Request telemetry query filtering.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class RequestQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public RequestQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task IngestRequest(AppInsightsTelemetryEnvelope envelope)
    {
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Query_WhenFilteringById_WithEquals_ShouldReturnMatchingRequest()
    {
        // Arrange
        var uniqueId = $"test-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        await IngestRequest(envelope);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueId, response.Requests[0].Request.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringById_WithContains_ShouldReturnMatchingRequest()
    {
        // Arrange
        var marker = $"marker-{Guid.NewGuid():N}";
        var uniqueId = $"prefix-{marker}-suffix";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        await IngestRequest(envelope);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = marker, CompareAs = StringCompareAsType.Contains }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueId, response.Requests[0].Request.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringByName_ShouldReturnMatchingRequest()
    {
        // Arrange
        var uniqueId = $"test-{Guid.NewGuid():N}";
        var uniqueName = $"GET /api/{uniqueId}";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Name = uniqueName;
        await IngestRequest(envelope);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty { Compare = uniqueName, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueId, response.Requests[0].Request.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringBySuccess_ShouldReturnMatchingRequest()
    {
        // Arrange
        var uniqueId = $"test-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Success = true;
        envelope.Data!.BaseData!.ResponseCode = "200";
        await IngestRequest(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var successFilter = new Where
        {
            Property = new PropertyFilter
            {
                Success = new BoolProperty { Compare = true, CompareAs = BoolCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, successFilter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.True(response.Requests[0].Request.Success);
    }

    [Fact]
    public async Task Query_WhenNoMatchingRequests_ShouldReturnEmptyResult()
    {
        // Arrange - use a non-existent ID
        var nonExistentId = $"non-existent-{Guid.NewGuid():N}";

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = nonExistentId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Empty(response.Requests);
    }
}
