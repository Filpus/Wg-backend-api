using Tests;
using Xunit;
using Wg_backend_api.Services;
using Moq;
using System.Net;
using XAssert = Xunit.Assert;
using Wg_backend_api.DTO;
using Newtonsoft.Json;
using System.Text;

namespace Tests.Api;

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

        var _factory = new TestingWebAppFactory(db.ConnectionString, schema: "game_1", nation: "1", mockSession);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetArmies()
    {
        var response = await _client.GetAsync("/api/armies");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetArmies_ValidId()
    {
        var response = await _client.GetAsync("/api/armies/1");

        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetArmy_InvalidId()
    {
        var response = await _client.GetAsync("/api/armies/99999");

        XAssert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostArmy_ValidData()
    {
        var dto = new CreateArmyDTO
        {
            Name = "303rd Squadron",
            LocationId = 1,
            NationId = null,
            IsNaval = false
        };

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/armies", content);

        response.EnsureSuccessStatusCode();

        XAssert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        XAssert.Contains("303rd Squadron", responseBody);
    }

    [Fact]
    public async Task PostArmy_InvalidLocationData()
    {
        var dto = new CreateArmyDTO
        {
            Name = "303rd Squadron",
            LocationId = null,
            NationId = null,
            IsNaval = false
        };

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/armies", content);

        XAssert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostArmy_InvalidDataName()
    {
        var dto = new CreateArmyDTO
        {
            Name = "303rd Squadron",
            LocationId = 1,
            NationId = null,
            IsNaval = false
        };

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/armies", content);

        XAssert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
