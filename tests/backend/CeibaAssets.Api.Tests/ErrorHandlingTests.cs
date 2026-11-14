using System.Net;

namespace CeibaAssets.Api.Tests;

public sealed class ErrorHandlingTests(CeibaApiFactory factory) : IClassFixture<CeibaApiFactory>
{
    [Fact]
    public async Task UnknownRoute_ReturnsUnauthorizedForAnonymous()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            new Uri("/v1/no-existe", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Con política de reserva RequireAuthenticatedUser, un anónimo recibe 401
        // (no se revela si la ruta existe).
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
    }

    [Theory]
    [InlineData("/swagger/v1/swagger.json")]
    [InlineData("/swagger/index.html")]
    public async Task OpenApiEndpoints_AreNotExposedOutsideDevelopment(string path)
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.True(
            response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Unauthorized,
            $"Código inesperado: {response.StatusCode}");
    }
}
