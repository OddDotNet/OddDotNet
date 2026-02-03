using System.Text;
using System.Text.Json;
using OddDotNet.Proto.AppInsights.V1;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for Trace telemetry query filtering.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class TraceQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public TraceQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task IngestTrace(AppInsightsTelemetryEnvelope envelope)
    {
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Query_WhenFilteringByMessage_WithEquals_ShouldReturnMatchingTrace()
    {
        // Arrange
        var uniqueMessage = $"test-trace-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateTraceEnvelope();
        envelope.Data!.BaseData!.Message = uniqueMessage;
        await IngestTrace(envelope);

        var filter = new TraceWhere
        {
            Property = new TracePropertyFilter
            {
                Message = new StringProperty { Compare = uniqueMessage, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiTraceQueryServiceClient.QueryAsync(
            new TraceQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Traces);
        Assert.Equal(uniqueMessage, response.Traces[0].Trace.Message);
    }

    [Fact]
    public async Task Query_WhenFilteringByMessage_WithContains_ShouldReturnMatchingTrace()
    {
        // Arrange
        var marker = $"marker-{Guid.NewGuid():N}";
        var uniqueMessage = $"User logged in with {marker} session";
        var envelope = AppInsightsHelpers.CreateTraceEnvelope();
        envelope.Data!.BaseData!.Message = uniqueMessage;
        await IngestTrace(envelope);

        var filter = new TraceWhere
        {
            Property = new TracePropertyFilter
            {
                Message = new StringProperty { Compare = marker, CompareAs = StringCompareAsType.Contains }
            }
        };

        // Act
        var response = await _fixture.AiTraceQueryServiceClient.QueryAsync(
            new TraceQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Traces);
        Assert.Equal(uniqueMessage, response.Traces[0].Trace.Message);
    }

    [Fact]
    public async Task Query_WhenFilteringBySeverityLevel_ShouldReturnMatchingTrace()
    {
        // Arrange
        var uniqueMessage = $"test-trace-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateTraceEnvelope();
        envelope.Data!.BaseData!.Message = uniqueMessage;
        envelope.Data!.BaseData!.SeverityLevel = 2; // Warning
        await IngestTrace(envelope);

        var messageFilter = new TraceWhere
        {
            Property = new TracePropertyFilter
            {
                Message = new StringProperty { Compare = uniqueMessage, CompareAs = StringCompareAsType.Equals }
            }
        };
        var severityFilter = new TraceWhere
        {
            Property = new TracePropertyFilter
            {
                SeverityLevel = new SeverityLevelProperty { Compare = SeverityLevel.Warning, CompareAs = EnumCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiTraceQueryServiceClient.QueryAsync(
            new TraceQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { messageFilter, severityFilter }
            });

        // Assert
        Assert.Single(response.Traces);
        Assert.Equal(SeverityLevel.Warning, response.Traces[0].Trace.SeverityLevel);
    }

    [Fact]
    public async Task Query_WhenNoMatchingTraces_ShouldReturnEmptyResult()
    {
        // Arrange - use a non-existent message
        var nonExistentMessage = $"non-existent-trace-{Guid.NewGuid():N}";

        var filter = new TraceWhere
        {
            Property = new TracePropertyFilter
            {
                Message = new StringProperty { Compare = nonExistentMessage, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiTraceQueryServiceClient.QueryAsync(
            new TraceQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Empty(response.Traces);
    }
}
