using Xunit;
using System.Net;
using Moq;
using Wg_backend_api.Services;
using Tests;
using XAssert = Xunit.Assert;

[Collection("Database collection")]
public class ApiTradeTests
{
    private readonly HttpClient _client;

    public ApiTradeTests(DatabaseFixture db)
    {
        var mockSession = new Mock<ISessionDataService>();
        mockSession.Setup(s => s.GetNation()).Returns("1");
        mockSession.Setup(s => s.GetSchema()).Returns("game_1");
        mockSession.Setup(s => s.GetRole()).Returns("Player");

        var _factory = new TestingWebAppFactory(db.ConnectionString, schema: "game_1", nation: "1",mockSession);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetOfferedTradeAgreements_WithoutNationId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/trade/OfferedTradeAgreements");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetOfferedTradeAgreements_WithNationId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/trade/OfferedTradeAgreements/1");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetReceivedTradeAgreements_WithoutNationId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/trade/ReceivedTradeAgreements");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetReceivedTradeAgreements_WithNationId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/trade/ReceivedTradeAgreements/1");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetOfferedTradeAgreements_IsAuthenticated_ReturnsData()
    {
        var authResponse = await _client.GetAsync("/api/auth/status");
        authResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/trade/OfferedTradeAgreements");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetReceivedTradeAgreements_IsAuthenticated_ReturnsData()
    {
        var authResponse = await _client.GetAsync("/api/auth/status");
        authResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/trade/ReceivedTradeAgreements");
        response.EnsureSuccessStatusCode();
    }
}