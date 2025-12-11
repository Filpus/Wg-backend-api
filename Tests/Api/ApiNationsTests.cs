using Xunit;
using System.Net;
using Moq;
using Wg_backend_api.Services;
using XAssert = Xunit.Assert;

namespace Tests.Api;

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
    public async Task GetNation()
    {
        var response = await _client.GetAsync("/api/nations");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetNation_ValidId()
    {
        var response = await _client.GetAsync("/api/nations/1");

        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetNation_InvalidId()
    {
        var response = await _client.GetAsync("/api/nations/99999");

        XAssert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetNation_ValidNationStructure()
    {
        var response = await _client.GetAsync("/api/nations");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        XAssert.Contains("id", json.ToLower());
        XAssert.Contains("name", json.ToLower());
    }

    [Fact]
    public async Task GetNation_ValidNationData()
    {
        var response = await _client.GetAsync("/api/nations/1");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        XAssert.NotEmpty(json);
        XAssert.Contains("id", json.ToLower());
    }

    [Fact]
    public async Task PostNation_ValidData()
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("Isengard"), "Name" },
            { new StringContent("1"), "ReligionId" },
            { new StringContent("1"), "CultureId" },
            { new StringContent(""), "Flag" },
            { new StringContent("#FF0000"), "Color" },
        };

        var response = await _client.PostAsync("/api/Nations", content);

        response.EnsureSuccessStatusCode();

        XAssert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        XAssert.Contains("Isengard", responseBody);

        var nationResponse = await _client.GetAsync("/api/Nations");
        response.EnsureSuccessStatusCode();

        var nationsJson = await response.Content.ReadAsStringAsync();

        XAssert.Contains("Isengard", nationsJson);
    }

    [Fact]
    public async Task PostNation_InvalidData()
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("Isengard"), "Name" },
            { new StringContent(""), "ReligionId" },
            { new StringContent(""), "CultureId" },
            { new StringContent(""), "Flag" },
            { new StringContent("#FF0000"), "Color" },
        };

        var response = await _client.PostAsync("/api/Nations", content);

        XAssert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
