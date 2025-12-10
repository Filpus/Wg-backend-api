using Xunit;
using System.Net;
using Moq;
using Wg_backend_api.Services;
using Tests;
using XAssert = Xunit.Assert;

[Collection("Database collection")]
public class ApiNationsTests
{
    private readonly HttpClient _client;

    public ApiNationsTests(DatabaseFixture db)
    {
        var mockSession = new Mock<ISessionDataService>();
        mockSession.Setup(s => s.GetNation()).Returns("1");
        mockSession.Setup(s => s.GetSchema()).Returns("game_1");
        mockSession.Setup(s => s.GetRole()).Returns("Player");

        var _factory = new TestingWebAppFactory(db.ConnectionString, schema: "game_1", nation: "1", mockSession);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetNations_WithoutId_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/nations");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetNations_WithValidId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/nations/1");

        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetNations_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/nations/99999");

        XAssert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetNations_IsAuthenticated_ReturnsData()
    {
        var authResponse = await _client.GetAsync("/api/auth/status");
        authResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/nations");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetNations_ReturnsValidNationStructure()
    {
        var response = await _client.GetAsync("/api/nations");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        
        XAssert.Contains("id", json.ToLower());
        XAssert.Contains("name", json.ToLower());
    }

    [Fact]
    public async Task GetNationById_ReturnsValidNationData()
    {
        var response = await _client.GetAsync("/api/nations/1");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        XAssert.NotEmpty(json);
        XAssert.Contains("id", json.ToLower());
    }    
}
