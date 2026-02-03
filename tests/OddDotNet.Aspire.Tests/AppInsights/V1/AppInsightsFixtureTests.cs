namespace OddDotNet.Aspire.Tests.AppInsights.V1;

// Verifies AspireFixture exposes App Insights query service clients.
// These are simple smoke tests - full integration tests are in separate files.
public class AppInsightsFixtureTests : IClassFixture<AspireFixture>
{
    private readonly AspireFixture _fixture;

    public AppInsightsFixtureTests(AspireFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void AiRequestQueryServiceClient_ShouldBeAvailable()
    {
        Assert.NotNull(_fixture.AiRequestQueryServiceClient);
    }
    
    [Fact]
    public void AiDependencyQueryServiceClient_ShouldBeAvailable()
    {
        Assert.NotNull(_fixture.AiDependencyQueryServiceClient);
    }
    
    [Fact]
    public void AiExceptionQueryServiceClient_ShouldBeAvailable()
    {
        Assert.NotNull(_fixture.AiExceptionQueryServiceClient);
    }
    
    [Fact]
    public void AiTraceQueryServiceClient_ShouldBeAvailable()
    {
        Assert.NotNull(_fixture.AiTraceQueryServiceClient);
    }
    
    [Fact]
    public void AiEventQueryServiceClient_ShouldBeAvailable()
    {
        Assert.NotNull(_fixture.AiEventQueryServiceClient);
    }
    
    [Fact]
    public void AiMetricQueryServiceClient_ShouldBeAvailable()
    {
        Assert.NotNull(_fixture.AiMetricQueryServiceClient);
    }
    
    [Fact]
    public void AiPageViewQueryServiceClient_ShouldBeAvailable()
    {
        Assert.NotNull(_fixture.AiPageViewQueryServiceClient);
    }
    
    [Fact]
    public void AiAvailabilityQueryServiceClient_ShouldBeAvailable()
    {
        Assert.NotNull(_fixture.AiAvailabilityQueryServiceClient);
    }
}
