using System.Text;
using System.Text.Json;
using OddDotNet.Proto.AppInsights.V1.Metric;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for Metric telemetry query filtering.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class MetricQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public MetricQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task IngestMetric(AppInsightsTelemetryEnvelope envelope)
    {
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Query_WhenFilteringByName_WithEquals_ShouldReturnMatchingMetric()
    {
        // Arrange
        var uniqueName = $"test-metric-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreateMetricEnvelope();
        envelope.Data!.BaseData!.Metrics![0].Name = uniqueName;
        await IngestMetric(envelope);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty { Compare = uniqueName, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiMetricQueryServiceClient.QueryAsync(
            new MetricQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Metrics);
        Assert.Equal(uniqueName, response.Metrics[0].Metric.Name);
    }

    [Fact]
    public async Task Query_WhenFilteringByName_WithContains_ShouldReturnMatchingMetric()
    {
        // Arrange
        var marker = $"marker-{Guid.NewGuid():N}";
        var uniqueName = $"RequestsPerSecond_{marker}";
        var envelope = AppInsightsHelpers.CreateMetricEnvelope();
        envelope.Data!.BaseData!.Metrics![0].Name = uniqueName;
        await IngestMetric(envelope);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty { Compare = marker, CompareAs = StringCompareAsType.Contains }
            }
        };

        // Act
        var response = await _fixture.AiMetricQueryServiceClient.QueryAsync(
            new MetricQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.Metrics);
        Assert.Equal(uniqueName, response.Metrics[0].Metric.Name);
    }

    [Fact]
    public async Task Query_WhenFilteringByNamespace_ShouldReturnMatchingMetric()
    {
        // Arrange
        var uniqueName = $"test-metric-{Guid.NewGuid():N}";
        var metricNamespace = $"MyApp.Performance.{uniqueName}";
        var envelope = AppInsightsHelpers.CreateMetricEnvelope();
        envelope.Data!.BaseData!.Metrics![0].Name = uniqueName;
        envelope.Data!.BaseData!.Metrics![0].Namespace = metricNamespace;
        await IngestMetric(envelope);

        var nameFilter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty { Compare = uniqueName, CompareAs = StringCompareAsType.Equals }
            }
        };
        var namespaceFilter = new Where
        {
            Property = new PropertyFilter
            {
                MetricNamespace = new StringProperty { Compare = metricNamespace, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiMetricQueryServiceClient.QueryAsync(
            new MetricQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { nameFilter, namespaceFilter }
            });

        // Assert
        Assert.Single(response.Metrics);
        Assert.Equal(metricNamespace, response.Metrics[0].Metric.MetricNamespace);
    }

    [Fact]
    public async Task Query_WhenNoMatchingMetrics_ShouldReturnEmptyResult()
    {
        // Arrange - use a non-existent name
        var nonExistentName = $"non-existent-metric-{Guid.NewGuid():N}";

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty { Compare = nonExistentName, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiMetricQueryServiceClient.QueryAsync(
            new MetricQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Empty(response.Metrics);
    }
}
