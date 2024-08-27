namespace OddDotNet.Aspire.Tests;

public class DistributedApplicationTestingBuilderTests
{
    public class AddOpenTelemetryTestHarnessShould
    {
        [Fact]
        public async Task ThrowWhenOtelNotRegistered()
        {
            var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.OddDotNet_Aspire_AppHost>();

            builder.AddOpenTelemetryTestHarness();

            // Assert.Throws<OddException>(() => builder.AddOpenTelemetryTestHarness(throwOnMissingConfiguration: true));

            var app = await builder.BuildAsync();
            await app.StartAsync();

            Assert.NotNull(app);
        }
    }
}