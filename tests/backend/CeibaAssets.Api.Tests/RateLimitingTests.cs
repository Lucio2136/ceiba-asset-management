using System.Net;
using Microsoft.Extensions.Options;

namespace CeibaAssets.Api.Tests;

public sealed class RateLimitingTests
{
    [Fact]
    public async Task ExceedingPermitLimit_ReturnsTooManyRequestsProblemWithRetryAfter()
    {
        await using var factory = new CeibaApiFactory();
        await using var limitedFactory = factory.WithWebHostBuilder(
            builder => builder.UseSetting("RateLimiting:PermitLimit", "2"));
        using var client = limitedFactory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;
        var endpoint = new Uri("/v1/system/info", UriKind.Relative);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            using var allowed = await client.GetAsync(endpoint, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        }

        using var rejected = await client.GetAsync(endpoint, cancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal("application/problem+json", rejected.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(rejected.Headers.RetryAfter);
    }

    [Fact]
    public async Task HealthProbes_AreExemptFromRateLimiting()
    {
        await using var factory = new CeibaApiFactory();
        await using var limitedFactory = factory.WithWebHostBuilder(
            builder => builder.UseSetting("RateLimiting:PermitLimit", "1"));
        using var client = limitedFactory.CreateClient();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            using var response = await client.GetAsync(
                new Uri("/health/live", UriKind.Relative),
                TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task InvalidRateLimitConfiguration_FailsAtStartup()
    {
        await using var factory = new CeibaApiFactory();
        await using var misconfiguredFactory = factory.WithWebHostBuilder(
            builder => builder.UseSetting("RateLimiting:PermitLimit", "0"));

        Assert.Throws<OptionsValidationException>(() => misconfiguredFactory.CreateClient());
    }
}
