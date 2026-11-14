using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CeibaAssets.Contracts.Platform;

namespace CeibaAssets.Api.Tests;

public sealed class PlatformEndpointsTests(CeibaApiFactory factory) : IClassFixture<CeibaApiFactory>
{
    [Fact]
    public async Task GetSystemInfo_ReturnsApplicationNameAndSemanticVersion()
    {
        using var client = factory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(new Uri("/v1/system/info", UriKind.Relative), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var info = await response.Content.ReadFromJsonAsync<SystemInfoResponse>(cancellationToken);
        Assert.NotNull(info);
        Assert.Equal("CEIBA Asset Management", info.ApplicationName);
        Assert.Matches(@"^\d+\.\d+\.\d+", info.Version);
        Assert.DoesNotContain('+', info.Version);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthProbes_ReportHealthyWithoutInternalDetails(string path)
    {
        using var client = factory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var report = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal("Healthy", report.RootElement.GetProperty("status").GetString());
        Assert.False(report.RootElement.TryGetProperty("exception", out _));
    }
}
