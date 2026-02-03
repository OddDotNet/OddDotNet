using System.Text;
using System.Text.Json;
using OddDotNet.Proto.AppInsights.V1;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for Exception telemetry query filtering.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class ExceptionQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public ExceptionQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task IngestException(AppInsightsTelemetryEnvelope envelope)
    {
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Query_WhenFilteringByProblemId_WithEquals_ShouldReturnMatchingException()
    {
        // Arrange
        var uniqueProblemId = $"test-exc-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateExceptionEnvelope();
        envelope.Data!.BaseData!.ProblemId = uniqueProblemId;
        await IngestException(envelope);

        var filter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                ProblemId = new StringProperty { Compare = uniqueProblemId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiExceptionQueryServiceClient.QueryAsync(
            new ExceptionQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Exceptions);
        Assert.Equal(uniqueProblemId, response.Exceptions[0].Exception.ProblemId);
    }

    [Fact]
    public async Task Query_WhenFilteringByProblemId_WithContains_ShouldReturnMatchingException()
    {
        // Arrange
        var marker = $"marker-{Guid.NewGuid():N}";
        var uniqueProblemId = $"NullRef_{marker}_UserController";
        var envelope = AppInsightsHelpers.CreateExceptionEnvelope();
        envelope.Data!.BaseData!.ProblemId = uniqueProblemId;
        await IngestException(envelope);

        var filter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                ProblemId = new StringProperty { Compare = marker, CompareAs = StringCompareAsType.Contains }
            }
        };

        // Act
        var response = await _fixture.AiExceptionQueryServiceClient.QueryAsync(
            new ExceptionQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Exceptions);
        Assert.Equal(uniqueProblemId, response.Exceptions[0].Exception.ProblemId);
    }

    [Fact]
    public async Task Query_WhenFilteringBySeverityLevel_ShouldReturnMatchingException()
    {
        // Arrange
        var uniqueProblemId = $"test-exc-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateExceptionEnvelope();
        envelope.Data!.BaseData!.ProblemId = uniqueProblemId;
        envelope.Data!.BaseData!.SeverityLevel = 3; // Error
        await IngestException(envelope);

        var problemIdFilter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                ProblemId = new StringProperty { Compare = uniqueProblemId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var severityFilter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                SeverityLevel = new SeverityLevelProperty { Compare = SeverityLevel.Error, CompareAs = EnumCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiExceptionQueryServiceClient.QueryAsync(
            new ExceptionQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { problemIdFilter, severityFilter }
            });

        // Assert
        Assert.Single(response.Exceptions);
        Assert.Equal(SeverityLevel.Error, response.Exceptions[0].Exception.SeverityLevel);
    }

    [Fact]
    public async Task Query_WhenFilteringByExceptionTypeName_ShouldReturnMatchingException()
    {
        // Arrange
        var uniqueProblemId = $"test-exc-{Guid.NewGuid():N}";
        var exceptionTypeName = "System.ArgumentNullException";
        var envelope = AppInsightsHelpers.CreateExceptionEnvelope();
        envelope.Data!.BaseData!.ProblemId = uniqueProblemId;
        envelope.Data!.BaseData!.Exceptions = new List<AppInsightsExceptionDetails>
        {
            new()
            {
                Id = 1,
                TypeName = exceptionTypeName,
                Message = "Value cannot be null",
                HasFullStack = true,
                Stack = "   at Test.Method()"
            }
        };
        await IngestException(envelope);

        var problemIdFilter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                ProblemId = new StringProperty { Compare = uniqueProblemId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var typeFilter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                ExceptionDetails = new ExceptionDetailsFilter
                {
                    TypeName = new StringProperty { Compare = exceptionTypeName, CompareAs = StringCompareAsType.Equals }
                }
            }
        };

        // Act
        var response = await _fixture.AiExceptionQueryServiceClient.QueryAsync(
            new ExceptionQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { problemIdFilter, typeFilter }
            });

        // Assert
        Assert.Single(response.Exceptions);
        Assert.Contains(response.Exceptions[0].Exception.Exceptions, ex => ex.TypeName == exceptionTypeName);
    }

    [Fact]
    public async Task Query_WhenFilteringByExceptionMessage_ShouldReturnMatchingException()
    {
        // Arrange
        var uniqueProblemId = $"test-exc-{Guid.NewGuid():N}";
        var exceptionMessage = $"Parameter '{uniqueProblemId}' cannot be null";
        var envelope = AppInsightsHelpers.CreateExceptionEnvelope();
        envelope.Data!.BaseData!.ProblemId = uniqueProblemId;
        envelope.Data!.BaseData!.Exceptions = new List<AppInsightsExceptionDetails>
        {
            new()
            {
                Id = 1,
                TypeName = "System.ArgumentNullException",
                Message = exceptionMessage,
                HasFullStack = true,
                Stack = "   at Test.Method()"
            }
        };
        await IngestException(envelope);

        var problemIdFilter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                ProblemId = new StringProperty { Compare = uniqueProblemId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var messageFilter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                ExceptionDetails = new ExceptionDetailsFilter
                {
                    Message = new StringProperty { Compare = exceptionMessage, CompareAs = StringCompareAsType.Equals }
                }
            }
        };

        // Act
        var response = await _fixture.AiExceptionQueryServiceClient.QueryAsync(
            new ExceptionQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { problemIdFilter, messageFilter }
            });

        // Assert
        Assert.Single(response.Exceptions);
        Assert.Contains(response.Exceptions[0].Exception.Exceptions, ex => ex.Message == exceptionMessage);
    }

    [Fact]
    public async Task Query_WhenNoMatchingExceptions_ShouldReturnEmptyResult()
    {
        // Arrange - use a non-existent problem ID
        var nonExistentProblemId = $"non-existent-exc-{Guid.NewGuid():N}";

        var filter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                ProblemId = new StringProperty { Compare = nonExistentProblemId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiExceptionQueryServiceClient.QueryAsync(
            new ExceptionQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Empty(response.Exceptions);
    }
}
