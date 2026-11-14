namespace CeibaAssets.Api.Tests;

public sealed class CorrelationIdTests(CeibaApiFactory factory) : IClassFixture<CeibaApiFactory>
{
    private const string HeaderName = "X-Correlation-ID";

    public static TheoryData<string> UnsafeCorrelationIds =>
    [
        "contiene espacios",
        "<script>alert(1)</script>",
        new string('a', 65),
    ];

    [Fact]
    public async Task ValidCorrelationId_IsEchoedInResponse()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/v1/system/info", UriKind.Relative));
        request.Headers.Add(HeaderName, "pedido-123.abc:XYZ_9");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal("pedido-123.abc:XYZ_9", Assert.Single(response.Headers.GetValues(HeaderName)));
    }

    [Theory]
    [MemberData(nameof(UnsafeCorrelationIds))]
    public async Task UnsafeCorrelationId_IsReplacedWithGeneratedValue(string unsafeValue)
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/v1/system/info", UriKind.Relative));
        request.Headers.TryAddWithoutValidation(HeaderName, unsafeValue);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        var correlationId = Assert.Single(response.Headers.GetValues(HeaderName));
        Assert.NotEqual(unsafeValue, correlationId);
        Assert.Matches("^[0-9a-f]{32}$", correlationId);
    }

    [Fact]
    public async Task MissingCorrelationId_IsGenerated()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            new Uri("/v1/system/info", UriKind.Relative),
            TestContext.Current.CancellationToken);

        Assert.Matches("^[0-9a-f]{32}$", Assert.Single(response.Headers.GetValues(HeaderName)));
    }
}
