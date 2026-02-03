using System.Text;
using System.Text.Json;
using OddDotNet.Proto.AppInsights.V1.PageView;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for PageView telemetry query filtering.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class PageViewQueryTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public PageViewQueryTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task IngestPageView(AppInsightsTelemetryEnvelope envelope)
    {
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Query_WhenFilteringById_WithEquals_ShouldReturnMatchingPageView()
    {
        // Arrange
        var uniqueId = $"test-pv-{Guid.NewGuid():N}";
        var envelope = AppInsightsHelpers.CreatePageViewEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        await IngestPageView(envelope);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiPageViewQueryServiceClient.QueryAsync(
            new PageViewQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.PageViews);
        Assert.Equal(uniqueId, response.PageViews[0].PageView.Id);
    }

    [Fact]
    public async Task Query_WhenFilteringByName_WithEquals_ShouldReturnMatchingPageView()
    {
        // Arrange
        var uniqueId = $"test-pv-{Guid.NewGuid():N}";
        var uniqueName = $"Dashboard_{uniqueId}";
        var envelope = AppInsightsHelpers.CreatePageViewEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Name = uniqueName;
        await IngestPageView(envelope);

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Name = new StringProperty { Compare = uniqueName, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiPageViewQueryServiceClient.QueryAsync(
            new PageViewQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Single(response.PageViews);
        Assert.Equal(uniqueName, response.PageViews[0].PageView.Name);
    }

    [Fact]
    public async Task Query_WhenFilteringByUrl_WithContains_ShouldReturnMatchingPageView()
    {
        // Arrange
        var marker = $"marker-{Guid.NewGuid():N}";
        var uniqueId = $"test-pv-{Guid.NewGuid():N}";
        var uniqueUrl = $"https://myapp.com/{marker}/dashboard";
        var envelope = AppInsightsHelpers.CreatePageViewEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.Url = uniqueUrl;
        await IngestPageView(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var urlFilter = new Where
        {
            Property = new PropertyFilter
            {
                Url = new StringProperty { Compare = marker, CompareAs = StringCompareAsType.Contains }
            }
        };

        // Act
        var response = await _fixture.AiPageViewQueryServiceClient.QueryAsync(
            new PageViewQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, urlFilter }
            });

        // Assert
        Assert.Single(response.PageViews);
        Assert.Equal(uniqueUrl, response.PageViews[0].PageView.Url);
    }

    [Fact]
    public async Task Query_WhenFilteringByReferrerUri_ShouldReturnMatchingPageView()
    {
        // Arrange
        var uniqueId = $"test-pv-{Guid.NewGuid():N}";
        var referrerUri = $"https://google.com/search?q={uniqueId}";
        var envelope = AppInsightsHelpers.CreatePageViewEnvelope();
        envelope.Data!.BaseData!.Id = uniqueId;
        envelope.Data!.BaseData!.ReferrerUri = referrerUri;
        await IngestPageView(envelope);

        var idFilter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var referrerFilter = new Where
        {
            Property = new PropertyFilter
            {
                ReferrerUri = new StringProperty { Compare = referrerUri, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiPageViewQueryServiceClient.QueryAsync(
            new PageViewQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { idFilter, referrerFilter }
            });

        // Assert
        Assert.Single(response.PageViews);
        Assert.Equal(referrerUri, response.PageViews[0].PageView.ReferrerUri);
    }

    [Fact]
    public async Task Query_WhenNoMatchingPageViews_ShouldReturnEmptyResult()
    {
        // Arrange - use a non-existent ID
        var nonExistentId = $"non-existent-pv-{Guid.NewGuid():N}";

        var filter = new Where
        {
            Property = new PropertyFilter
            {
                Id = new StringProperty { Compare = nonExistentId, CompareAs = StringCompareAsType.Equals }
            }
        };

        // Act
        var response = await _fixture.AiPageViewQueryServiceClient.QueryAsync(
            new PageViewQueryRequest
            {
                Take = new Take { TakeFirst = new TakeFirst() },
                Filters = { filter }
            });

        // Assert
        Assert.Empty(response.PageViews);
    }
}
