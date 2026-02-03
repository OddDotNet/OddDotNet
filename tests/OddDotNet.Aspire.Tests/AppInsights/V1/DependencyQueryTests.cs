using System.Text;
using System.Text.Json;
using OddDotNet.Proto.AppInsights.V1;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for Dependency telemetry query filtering.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class DependencyQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public DependencyQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task IngestDependency(AppInsightsTelemetryEnvelope envelope)
    {
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Query_WhenFilteringById_WithEquals_ShouldReturnMatchingDependency()
    {
        // Arrange
        var uniqueId = $"test-dep-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateDependencyEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        await IngestDependency(envelope);

        var filter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiDependencyQueryServiceClient.QueryAsync(
            new DependencyQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Dependencies);
        Assert.Equal(uniqueId, response.Dependencies[0].Dependency.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringById_WithContains_ShouldReturnMatchingDependency()
    {
        // Arrange
        var marker = $"marker-{Guid.NewGuid():N}";
        var uniqueId = $"prefix-{marker}-suffix";
        var envelope = AppInsightsHelpers.CreateDependencyEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        await IngestDependency(envelope);

        var filter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Id = new StringProperty { Compare = marker, CompareAs = StringCompareAsType.Contains }
            }
        };

        // Act
        var response = await _fixture.AiDependencyQueryServiceClient.QueryAsync(
            new DependencyQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Dependencies);
        Assert.Equal(uniqueId, response.Dependencies[0].Dependency.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringByName_ShouldReturnMatchingDependency()
    {
        // Arrange
        var uniqueId = $"test-dep-{Guid.NewGuid():N}";
        var uniqueName = $"GET /api/{uniqueId}";
        var envelope = AppInsightsHelpers.CreateDependencyEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Name = uniqueName;
        await IngestDependency(envelope);

        var filter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Name = new StringProperty { Compare = uniqueName, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiDependencyQueryServiceClient.QueryAsync(
            new DependencyQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Dependencies);
        Assert.Equal(uniqueId, response.Dependencies[0].Dependency.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringBySuccess_ShouldReturnMatchingDependency()
    {
        // Arrange
        var uniqueId = $"test-dep-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateDependencyEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Success = true;
        envelope.Data!.BaseData!.ResultCode = "200";
        await IngestDependency(envelope);

        var idFilter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var successFilter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Success = new BoolProperty { Compare = true, CompareAs = BoolCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiDependencyQueryServiceClient.QueryAsync(
            new DependencyQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, successFilter }
            });

        // Assert
        Assert.Single(response.Dependencies);
        Assert.True(response.Dependencies[0].Dependency.Success);
    }

    [Fact]
    public async Task Query_WhenFilteringByType_ShouldReturnMatchingDependency()
    {
        // Arrange
        var uniqueId = $"test-dep-{Guid.NewGuid():N}";
        var dependencyType = "SQL";
        var envelope = AppInsightsHelpers.CreateDependencyEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Type = dependencyType;
        await IngestDependency(envelope);

        var idFilter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var typeFilter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Type = new StringProperty { Compare = dependencyType, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiDependencyQueryServiceClient.QueryAsync(
            new DependencyQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, typeFilter }
            });

        // Assert
        Assert.Single(response.Dependencies);
        Assert.Equal(dependencyType, response.Dependencies[0].Dependency.Type);
    }

    [Fact]
    public async Task Query_WhenFilteringByTarget_ShouldReturnMatchingDependency()
    {
        // Arrange
        var uniqueId = $"test-dep-{Guid.NewGuid():N}";
        var target = $"sql-server-{uniqueId}.database.windows.net";
        var envelope = AppInsightsHelpers.CreateDependencyEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Target = target;
        await IngestDependency(envelope);

        var idFilter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var targetFilter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Target = new StringProperty { Compare = target, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiDependencyQueryServiceClient.QueryAsync(
            new DependencyQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, targetFilter }
            });

        // Assert
        Assert.Single(response.Dependencies);
        Assert.Equal(target, response.Dependencies[0].Dependency.Target);
    }

    [Fact]
    public async Task Query_WhenFilteringByResultCode_ShouldReturnMatchingDependency()
    {
        // Arrange
        var uniqueId = $"test-dep-{Guid.NewGuid():N}";
        var resultCode = "404";
        var envelope = AppInsightsHelpers.CreateDependencyEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.ResultCode = resultCode;
        envelope.Data!.BaseData!.Success = false;
        await IngestDependency(envelope);

        var idFilter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var resultCodeFilter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                ResultCode = new StringProperty { Compare = resultCode, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiDependencyQueryServiceClient.QueryAsync(
            new DependencyQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, resultCodeFilter }
            });

        // Assert
        Assert.Single(response.Dependencies);
        Assert.Equal(resultCode, response.Dependencies[0].Dependency.ResultCode);
    }

    [Fact]
    public async Task Query_WhenNoMatchingDependencies_ShouldReturnEmptyResult()
    {
        // Arrange - use a non-existent ID
        var nonExistentId = $"non-existent-dep-{Guid.NewGuid():N}";

        var filter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Id = new StringProperty { Compare = nonExistentId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiDependencyQueryServiceClient.QueryAsync(
            new DependencyQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Empty(response.Dependencies);
    }
}
