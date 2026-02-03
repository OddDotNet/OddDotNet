using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OddDotNet.Proto.Common.V1;
using OddDotNet.Services.AppInsights;

// Request types
using RequestWhere = OddDotNet.Proto.AppInsights.V1.Request.Where;
using RequestPropertyFilter = OddDotNet.Proto.AppInsights.V1.Request.PropertyFilter;
using RequestQueryRequest = OddDotNet.Proto.AppInsights.V1.Request.RequestQueryRequest;

// Dependency types
using DependencyWhere = OddDotNet.Proto.AppInsights.V1.Dependency.Where;
using DependencyPropertyFilter = OddDotNet.Proto.AppInsights.V1.Dependency.PropertyFilter;
using DependencyQueryRequest = OddDotNet.Proto.AppInsights.V1.Dependency.DependencyQueryRequest;

// Exception types
using ExceptionWhere = OddDotNet.Proto.AppInsights.V1.Exception.Where;
using ExceptionPropertyFilter = OddDotNet.Proto.AppInsights.V1.Exception.PropertyFilter;
using ExceptionQueryRequest = OddDotNet.Proto.AppInsights.V1.Exception.ExceptionQueryRequest;

// Trace types
using TraceWhere = OddDotNet.Proto.AppInsights.V1.Trace.Where;
using TracePropertyFilter = OddDotNet.Proto.AppInsights.V1.Trace.PropertyFilter;
using TraceQueryRequest = OddDotNet.Proto.AppInsights.V1.Trace.TraceQueryRequest;

// Event types
using EventWhere = OddDotNet.Proto.AppInsights.V1.Event.Where;
using EventPropertyFilter = OddDotNet.Proto.AppInsights.V1.Event.PropertyFilter;
using EventQueryRequest = OddDotNet.Proto.AppInsights.V1.Event.EventQueryRequest;

// Metric types
using MetricWhere = OddDotNet.Proto.AppInsights.V1.Metric.Where;
using MetricPropertyFilter = OddDotNet.Proto.AppInsights.V1.Metric.PropertyFilter;
using MetricQueryRequest = OddDotNet.Proto.AppInsights.V1.Metric.MetricQueryRequest;

// PageView types
using PageViewWhere = OddDotNet.Proto.AppInsights.V1.PageView.Where;
using PageViewPropertyFilter = OddDotNet.Proto.AppInsights.V1.PageView.PropertyFilter;
using PageViewQueryRequest = OddDotNet.Proto.AppInsights.V1.PageView.PageViewQueryRequest;

// Availability types
using AvailabilityWhere = OddDotNet.Proto.AppInsights.V1.Availability.Where;
using AvailabilityPropertyFilter = OddDotNet.Proto.AppInsights.V1.Availability.PropertyFilter;
using AvailabilityQueryRequest = OddDotNet.Proto.AppInsights.V1.Availability.AvailabilityQueryRequest;

namespace OddDotNet.Aspire.Tests.AppInsights.V1;

/// <summary>
/// Integration tests for the App Insights /v2/track ingestion endpoint.
/// Tests verify telemetry is accepted and stored correctly.
/// Each test uses unique identifiers to be isolated.
/// </summary>
public class AppInsightsIngestionTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public AppInsightsIngestionTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    #region Request Telemetry Tests

    [Fact]
    public async Task Track_WhenRequestTelemetrySent_ShouldStoreAndBeQueryable()
    {
        // Arrange
        var envelope = AppInsightsHelpers.CreateRequestEnvelope();
        var uniqueId = $"ingest-req-{Guid.NewGuid():N}";
        envelope.Data!.BaseData!.Id = uniqueId;
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var filter = new RequestWhere
        {
            Property = new RequestPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var queryResponse = await _fixture.AiRequestQueryServiceClient.QueryAsync(
            new RequestQueryRequest { Take = new Take { TakeFirst = new TakeFirst() }, Filters = { filter } });
        
        Assert.Single(queryResponse.Requests);
        Assert.Equal(uniqueId, queryResponse.Requests[0].Request.Id);
    }

    #endregion

    #region Dependency Telemetry Tests

    [Fact]
    public async Task Track_WhenDependencyTelemetrySent_ShouldStoreAndBeQueryable()
    {
        // Arrange
        var envelope = AppInsightsHelpers.CreateDependencyEnvelope();
        var uniqueId = $"ingest-dep-{Guid.NewGuid():N}";
        envelope.Data!.BaseData!.Id = uniqueId;
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var filter = new DependencyWhere
        {
            Property = new DependencyPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var queryResponse = await _fixture.AiDependencyQueryServiceClient.QueryAsync(
            new DependencyQueryRequest { Take = new Take { TakeFirst = new TakeFirst() }, Filters = { filter } });
        
        Assert.Single(queryResponse.Dependencies);
        Assert.Equal(uniqueId, queryResponse.Dependencies[0].Dependency.Id);
    }

    #endregion

    #region Exception Telemetry Tests

    [Fact]
    public async Task Track_WhenExceptionTelemetrySent_ShouldStoreAndBeQueryable()
    {
        // Arrange
        var envelope = AppInsightsHelpers.CreateExceptionEnvelope();
        var uniqueProblemId = $"ingest-exc-{Guid.NewGuid():N}";
        envelope.Data!.BaseData!.ProblemId = uniqueProblemId;
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var filter = new ExceptionWhere
        {
            Property = new ExceptionPropertyFilter
            {
                ProblemId = new StringProperty { Compare = uniqueProblemId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var queryResponse = await _fixture.AiExceptionQueryServiceClient.QueryAsync(
            new ExceptionQueryRequest { Take = new Take { TakeFirst = new TakeFirst() }, Filters = { filter } });
        
        Assert.Single(queryResponse.Exceptions);
        Assert.Equal(uniqueProblemId, queryResponse.Exceptions[0].Exception.ProblemId);
    }

    #endregion

    #region Trace Telemetry Tests

    [Fact]
    public async Task Track_WhenTraceTelemetrySent_ShouldStoreAndBeQueryable()
    {
        // Arrange
        var envelope = AppInsightsHelpers.CreateTraceEnvelope();
        var uniqueMessage = $"ingest-trace-{Guid.NewGuid():N}";
        envelope.Data!.BaseData!.Message = uniqueMessage;
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var filter = new TraceWhere
        {
            Property = new TracePropertyFilter
            {
                Message = new StringProperty { Compare = uniqueMessage, CompareAs = StringCompareAsType.Equals }
            }
        };
        var queryResponse = await _fixture.AiTraceQueryServiceClient.QueryAsync(
            new TraceQueryRequest { Take = new Take { TakeFirst = new TakeFirst() }, Filters = { filter } });
        
        Assert.Single(queryResponse.Traces);
        Assert.Equal(uniqueMessage, queryResponse.Traces[0].Trace.Message);
    }

    #endregion

    #region Event Telemetry Tests

    [Fact]
    public async Task Track_WhenEventTelemetrySent_ShouldStoreAndBeQueryable()
    {
        // Arrange
        var envelope = AppInsightsHelpers.CreateEventEnvelope();
        var uniqueName = $"ingest-evt-{Guid.NewGuid():N}";
        envelope.Data!.BaseData!.Name = uniqueName;
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var filter = new EventWhere
        {
            Property = new EventPropertyFilter
            {
                Name = new StringProperty { Compare = uniqueName, CompareAs = StringCompareAsType.Equals }
            }
        };
        var queryResponse = await _fixture.AiEventQueryServiceClient.QueryAsync(
            new EventQueryRequest { Take = new Take { TakeFirst = new TakeFirst() }, Filters = { filter } });
        
        Assert.Single(queryResponse.Events);
        Assert.Equal(uniqueName, queryResponse.Events[0].Event.Name);
    }

    #endregion

    #region Metric Telemetry Tests

    [Fact]
    public async Task Track_WhenMetricTelemetrySent_ShouldStoreAndBeQueryable()
    {
        // Arrange
        var envelope = AppInsightsHelpers.CreateMetricEnvelope();
        var uniqueName = $"ingest-metric-{Guid.NewGuid():N}";
        envelope.Data!.BaseData!.Metrics![0].Name = uniqueName;
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var filter = new MetricWhere
        {
            Property = new MetricPropertyFilter
            {
                Name = new StringProperty { Compare = uniqueName, CompareAs = StringCompareAsType.Equals }
            }
        };
        var queryResponse = await _fixture.AiMetricQueryServiceClient.QueryAsync(
            new MetricQueryRequest { Take = new Take { TakeFirst = new TakeFirst() }, Filters = { filter } });
        
        Assert.Single(queryResponse.Metrics);
        Assert.Equal(uniqueName, queryResponse.Metrics[0].Metric.Name);
    }

    #endregion

    #region PageView Telemetry Tests

    [Fact]
    public async Task Track_WhenPageViewTelemetrySent_ShouldStoreAndBeQueryable()
    {
        // Arrange
        var envelope = AppInsightsHelpers.CreatePageViewEnvelope();
        var uniqueId = $"ingest-pv-{Guid.NewGuid():N}";
        envelope.Data!.BaseData!.Id = uniqueId;
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var filter = new PageViewWhere
        {
            Property = new PageViewPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var queryResponse = await _fixture.AiPageViewQueryServiceClient.QueryAsync(
            new PageViewQueryRequest { Take = new Take { TakeFirst = new TakeFirst() }, Filters = { filter } });
        
        Assert.Single(queryResponse.PageViews);
        Assert.Equal(uniqueId, queryResponse.PageViews[0].PageView.Id);
    }

    #endregion

    #region Availability Telemetry Tests

    [Fact]
    public async Task Track_WhenAvailabilityTelemetrySent_ShouldStoreAndBeQueryable()
    {
        // Arrange
        var envelope = AppInsightsHelpers.CreateAvailabilityEnvelope();
        var uniqueId = $"ingest-avail-{Guid.NewGuid():N}";
        envelope.Data!.BaseData!.Id = uniqueId;
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var filter = new AvailabilityWhere
        {
            Property = new AvailabilityPropertyFilter
            {
                Id = new StringProperty { Compare = uniqueId, CompareAs = StringCompareAsType.Equals }
            }
        };
        var queryResponse = await _fixture.AiAvailabilityQueryServiceClient.QueryAsync(
            new AvailabilityQueryRequest { Take = new Take { TakeFirst = new TakeFirst() }, Filters = { filter } });
        
        Assert.Single(queryResponse.Availabilities);
        Assert.Equal(uniqueId, queryResponse.Availabilities[0].Availability.Id);
    }

    #endregion

    #region Format Tests

    [Fact]
    public async Task Track_WhenJsonArraySent_ShouldProcessAllItems()
    {
        // Arrange
        var envelopes = new[]
        {
            AppInsightsHelpers.CreateRequestEnvelope(),
            AppInsightsHelpers.CreateDependencyEnvelope(),
            AppInsightsHelpers.CreateTraceEnvelope()
        };
        var json = JsonSerializer.Serialize(envelopes);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, responseJson.GetProperty("itemsReceived").GetInt32());
        Assert.Equal(3, responseJson.GetProperty("itemsAccepted").GetInt32());
    }

    [Fact]
    public async Task Track_WhenNdjsonSent_ShouldProcessAllItems()
    {
        // Arrange
        var envelope1 = AppInsightsHelpers.CreateRequestEnvelope();
        var envelope2 = AppInsightsHelpers.CreateEventEnvelope();
        var ndjson = $"{JsonSerializer.Serialize(envelope1)}\n{JsonSerializer.Serialize(envelope2)}";
        var content = new StringContent(ndjson, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, responseJson.GetProperty("itemsReceived").GetInt32());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Track_WhenEmptyBody_ShouldReturnBadRequest()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Track_WhenInvalidJson_ShouldReturnBadRequest()
    {
        // Arrange
        var content = new StringContent("{ invalid json }", Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Track_WhenUnknownTelemetryType_ShouldNotCrash()
    {
        // Arrange - create envelope with unknown baseType
        var envelope = new AppInsightsTelemetryEnvelope
        {
            Name = "Microsoft.ApplicationInsights.Unknown",
            Time = DateTime.UtcNow.ToString("O"),
            InstrumentationKey = Guid.NewGuid().ToString(),
            Data = new AppInsightsData
            {
                BaseType = "UnknownData",
                BaseData = new AppInsightsBaseData()
            }
        };
        var json = JsonSerializer.Serialize(envelope);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.HttpClient.PostAsync("/v2/track", content);

        // Assert - should succeed but not store anything
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}
