using Tests;
using Xunit;
using Wg_backend_api.Services;
using Moq;
using System.Net;
using XAssert = Xunit.Assert;

[Collection("Database collection")]
public class ApiControllersTests
{
    private readonly HttpClient _client;

    public ApiControllersTests(DatabaseFixture db)
    {
        var mockSession = new Mock<ISessionDataService>();
        mockSession.Setup(s => s.GetNation()).Returns("1");
        mockSession.Setup(s => s.GetSchema()).Returns("game_1");
        mockSession.Setup(s => s.GetRole()).Returns("Player");

        var _factory = new TestingWebAppFactory(db.ConnectionString, schema: "game_1", nation: "1",mockSession);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkAndData()
    {
        var response = await _client.GetAsync("/api/auth/status");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        var responsepop = await _client.GetAsync("/api/populations");
        responsepop.EnsureSuccessStatusCode();

        string jsonpop = await responsepop.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task GetPopulations_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/populations");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetPopulationById_WithValidId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/populations/1");

        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPopulationById_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/populations/99999");

        XAssert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPopulations_IsAuthenticated_ReturnsData()
    {
        var authResponse = await _client.GetAsync("/api/auth/status");
        authResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/populations");
        response.EnsureSuccessStatusCode();
    }
}

