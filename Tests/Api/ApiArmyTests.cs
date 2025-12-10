using Tests;
using Xunit;
using Wg_backend_api.Services;
using Moq;
using System.Net;
using XAssert = Xunit.Assert;

[Collection("Database collection")]
public class ApiArmyTests
{
    private readonly HttpClient _client;

    public ApiArmyTests(DatabaseFixture db)
    {
        var mockSession = new Mock<ISessionDataService>();
        mockSession.Setup(s => s.GetNation()).Returns("1");
        mockSession.Setup(s => s.GetSchema()).Returns("game_1");
        mockSession.Setup(s => s.GetRole()).Returns("Player");

        var _factory = new TestingWebAppFactory(db.ConnectionString, schema: "game_1", nation: "1",mockSession);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetArmies_WithoutId_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/armies");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetArmies_WithValidId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/armies/1");

        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetArmies_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/armies/99999");

        XAssert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetArmies_IsAuthenticated_ReturnsData()
    {
        var authResponse = await _client.GetAsync("/api/auth/status");
        authResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/armies");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetArmies_ReturnsValidArmyStructure()
    {
        var response = await _client.GetAsync("/api/armies");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        
        XAssert.Contains("armyId", json);
        // XAssert.Contains("ArmyId", json);
    }
}
