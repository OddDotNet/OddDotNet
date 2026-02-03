using System.Text;
using System.Text.Json;
using OddDotNet.Proto.AppInsights.V1;
using OddDotNet.Proto.AppInsights.V1.Request;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for querying by common context fields (operation, cloud, user, session).
/// Uses Request telemetry as the carrier, but context filtering works for all telemetry types.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class ContextQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public ContextQueryTests(AspireFixture fixture)
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

    #region Operation Context Tests

    [Fact]
    public async Task Query_WhenFilteringByOperationId_ShouldReturnMatchingTelemetry()
    {
        // Arrange
        var uniqueRequestId = $"test-ctx-{Guid.NewGuid():N}";
        var uniqueOperationId = $"op-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueRequestId;
        envelope.Tags!["ai.operation.id"] = uniqueOperationId;
        await IngestRequest(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueRequestId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var operationFilter = new Where
        {
            Envelope = new EnvelopeFilter
            {
                Context = new ContextFilter
                {
                    Operation = new OperationContextFilter
                    {
                        Id = new StringProperty { Compare = uniqueOperationId, CompareAs = StringCompareAsType.Equals }
                    }
                }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, operationFilter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueOperationId, response.Requests[0].Envelope.Context.Operation.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringByOperationName_ShouldReturnMatchingTelemetry()
    {
        // Arrange
        var uniqueRequestId = $"test-ctx-{Guid.NewGuid():N}";
        var uniqueOperationName = $"GET /api/users/{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueRequestId;
        envelope.Tags!["ai.operation.name"] = uniqueOperationName;
        await IngestRequest(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueRequestId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var operationFilter = new Where
        {
            Envelope = new EnvelopeFilter
            {
                Context = new ContextFilter
                {
                    Operation = new OperationContextFilter
                    {
                        Name = new StringProperty { Compare = uniqueOperationName, CompareAs = StringCompareAsType.Equals }
                    }
                }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, operationFilter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueOperationName, response.Requests[0].Envelope.Context.Operation.Name);
    }

    #endregion

    #region Cloud Context Tests

    [Fact]
    public async Task Query_WhenFilteringByCloudRoleName_ShouldReturnMatchingTelemetry()
    {
        // Arrange
        var uniqueRequestId = $"test-ctx-{Guid.NewGuid():N}";
        var uniqueRoleName = $"MyService-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueRequestId;
        envelope.Tags!["ai.cloud.roleName"] = uniqueRoleName;
        await IngestRequest(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueRequestId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var cloudFilter = new Where
        {
            Envelope = new EnvelopeFilter
            {
                Context = new ContextFilter
                {
                    Cloud = new CloudContextFilter
                    {
                        RoleName = new StringProperty { Compare = uniqueRoleName, CompareAs = StringCompareAsType.Equals }
                    }
                }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, cloudFilter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueRoleName, response.Requests[0].Envelope.Context.Cloud.RoleName);
    }

    [Fact]
    public async Task Query_WhenFilteringByCloudRoleInstance_ShouldReturnMatchingTelemetry()
    {
        // Arrange
        var uniqueRequestId = $"test-ctx-{Guid.NewGuid():N}";
        var uniqueRoleInstance = $"instance-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueRequestId;
        envelope.Tags!["ai.cloud.roleInstance"] = uniqueRoleInstance;
        await IngestRequest(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueRequestId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var cloudFilter = new Where
        {
            Envelope = new EnvelopeFilter
            {
                Context = new ContextFilter
                {
                    Cloud = new CloudContextFilter
                    {
                        RoleInstance = new StringProperty { Compare = uniqueRoleInstance, CompareAs = StringCompareAsType.Equals }
                    }
                }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, cloudFilter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueRoleInstance, response.Requests[0].Envelope.Context.Cloud.RoleInstance);
    }

    #endregion

    #region User Context Tests

    [Fact]
    public async Task Query_WhenFilteringByUserId_ShouldReturnMatchingTelemetry()
    {
        // Arrange
        var uniqueRequestId = $"test-ctx-{Guid.NewGuid():N}";
        var uniqueUserId = $"user-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueRequestId;
        envelope.Tags!["ai.user.id"] = uniqueUserId;
        await IngestRequest(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueRequestId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var userFilter = new Where
        {
            Envelope = new EnvelopeFilter
            {
                Context = new ContextFilter
                {
                    User = new UserContextFilter
                    {
                        Id = new StringProperty { Compare = uniqueUserId, CompareAs = StringCompareAsType.Equals }
                    }
                }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, userFilter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueUserId, response.Requests[0].Envelope.Context.User.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringByUserAuthenticatedId_ShouldReturnMatchingTelemetry()
    {
        // Arrange
        var uniqueRequestId = $"test-ctx-{Guid.NewGuid():N}";
        var uniqueAuthId = $"auth-{Guid.NewGuid():N}@example.com";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueRequestId;
        envelope.Tags!["ai.user.authUserId"] = uniqueAuthId;
        await IngestRequest(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueRequestId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var userFilter = new Where
        {
            Envelope = new EnvelopeFilter
            {
                Context = new ContextFilter
                {
                    User = new UserContextFilter
                    {
                        AuthenticatedId = new StringProperty { Compare = uniqueAuthId, CompareAs = StringCompareAsType.Equals }
                    }
                }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, userFilter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueAuthId, response.Requests[0].Envelope.Context.User.AuthenticatedId);
    }

    #endregion

    #region Session Context Tests

    [Fact]
    public async Task Query_WhenFilteringBySessionId_ShouldReturnMatchingTelemetry()
    {
        // Arrange
        var uniqueRequestId = $"test-ctx-{Guid.NewGuid():N}";
        var uniqueSessionId = $"session-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        envelope.Data!.BaseData!.Id = uniqueRequestId;
        envelope.Tags!["ai.session.id"] = uniqueSessionId;
        await IngestRequest(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueRequestId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var sessionFilter = new Where
        {
            Envelope = new EnvelopeFilter
            {
                Context = new ContextFilter
                {
                    Session = new SessionContextFilter
                    {
                        Id = new StringProperty { Compare = uniqueSessionId, CompareAs = StringCompareAsType.Equals }
                    }
                }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, sessionFilter }
            });

        // Assert
        Assert.Single(response.Requests);
        Assert.Equal(uniqueSessionId, response.Requests[0].Envelope.Context.Session.Id);
    }

    #endregion

    #region Negative Tests

    [Fact]
    public async Task Query_WhenFilteringByNonMatchingOperationId_ShouldReturnEmptyResult()
    {
        // Arrange
        var nonExistentOperationId = $"non-existent-op-{Guid.NewGuid():N}";

        var operationFilter = new Where
        {
            Envelope = new EnvelopeFilter
            {
                Context = new ContextFilter
                {
                    Operation = new OperationContextFilter
                    {
                        Id = new StringProperty { Compare = nonExistentOperationId, CompareAs = StringCompareAsType.Equals }
                    }
                }
            }
        };

        // Act
        var response = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { operationFilter }
            });

        // Assert
        Assert.Empty(response.Requests);
    }

    #endregion
}
