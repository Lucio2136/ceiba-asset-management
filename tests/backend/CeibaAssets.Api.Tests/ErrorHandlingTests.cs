using System.Net;
using System.Text.Json;

namespace CeibaAssets.Api.Tests;

public sealed class ErrorHandlingTests(CeibaApiFactory factory) : IClassFixture<CeibaApiFactory>
{
    [Fact]
    public async Task UnknownRoute_ReturnsProblemDetailsWithTraceAndCorrelation()
    {
        using var client = factory.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(new Uri("/v1/no-existe", UriKind.Relative), cancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal(404, problem.RootElement.GetProperty("status").GetInt32());
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));

        var correlationId = Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
        Assert.Equal(correlationId, problem.RootElement.GetProperty("correlationId").GetString());
    }

    [Theory]
    [InlineData("/swagger/v1/swagger.json")]
    [InlineData("/swagger/index.html")]
    public async Task OpenApiEndpoints_AreNotExposedOutsideDevelopment(string path)
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
