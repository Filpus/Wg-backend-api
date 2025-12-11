using Tests;
using Xunit;
using Wg_backend_api.Services;
using Moq;
using System.Net;
using XAssert = Xunit.Assert;
using Newtonsoft.Json;
using System.Text;
using Wg_backend_api.DTO;

namespace Tests.Api;

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

        var _factory = new TestingWebAppFactory(db.ConnectionString, schema: "game_1", nation: "1", mockSession);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetPopulations_OkList()
    {
        var response = await _client.GetAsync("/api/populations");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetPopulation_ValidId()
    {
        var response = await _client.GetAsync("/api/populations/1");

        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPopulation_InvalidId()
    {
        var response = await _client.GetAsync("/api/populations/2137");

        XAssert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostPopulation_ValidData()
    {
        var dto = new List<PopulationDTO>()
        { new PopulationDTO{
            ReligionId = 1,
            CultureId = 1,
            SocialGroupId = 1,
            LocationId = 1,
            Happiness = 75.5f,
            Volonteers = 1000
        }
        };

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/populations", content);

        response.EnsureSuccessStatusCode();

        XAssert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostPopulation_InvalidData()
    {
        var dto = new List<PopulationDTO>()
            { new PopulationDTO {
                ReligionId = 1,
                CultureId = 1,
                SocialGroupId = 2137,
                LocationId = 1,
                Happiness = 69f,
                Volonteers = 2137
            }
        };

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/populations", content);

        XAssert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

