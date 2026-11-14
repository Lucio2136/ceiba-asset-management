namespace CeibaAssets.Api.Tests;

public sealed class SecurityHeadersTests(CeibaApiFactory factory) : IClassFixture<CeibaApiFactory>
{
    [Theory]
    [InlineData("/v1/system/info")]
    [InlineData("/v1/no-existe")]
    public async Task Responses_IncludeSecurityHeaders(string path)
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("DENY", Assert.Single(response.Headers.GetValues("X-Frame-Options")));
        Assert.Equal("no-referrer", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
        Assert.Equal("same-origin", Assert.Single(response.Headers.GetValues("Cross-Origin-Resource-Policy")));
        Assert.Equal(
            "default-src 'none'; frame-ancestors 'none'",
            Assert.Single(response.Headers.GetValues("Content-Security-Policy")));
        Assert.True(response.Headers.CacheControl?.NoStore);
    }
}
