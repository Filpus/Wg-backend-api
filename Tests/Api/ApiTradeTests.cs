using Xunit;
using System.Net;
using Moq;
using Wg_backend_api.Services;
using Tests;
using XAssert = Xunit.Assert;
using Wg_backend_api.DTO;
using Newtonsoft.Json;
using System.Text;
using Wg_backend_api.Enums;

namespace Tests.Api;

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
    public async Task GetOfferedTradeAgreements()
    {
        var response = await _client.GetAsync("/api/trade/OfferedTradeAgreements");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetOfferedTradeAgreements_Id()
    {
        var response = await _client.GetAsync("/api/trade/OfferedTradeAgreements/1");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetReceivedTradeAgreements()
    {
        var response = await _client.GetAsync("/api/trade/ReceivedTradeAgreements");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task GetReceivedTradeAgreements_Id()
    {
        var response = await _client.GetAsync("/api/trade/ReceivedTradeAgreements/1");

        response.EnsureSuccessStatusCode();
        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        XAssert.NotEmpty(json);
    }

    [Fact]
    public async Task PostTrade_ValidData()
    {
        var dto = new OfferTradeAgreementDTO
        {
            ReceivingNationId = 5,
            Duration = 2,
            Description = "Im description",
            TradeStatus = TradeStatus.Pending,
            OfferedResources = new List<ResourceAmountDto>
            {
                new ResourceAmountDto { ResourceId = 1, Amount = 100 },
                new ResourceAmountDto { ResourceId = 2, Amount = 200 }
            },
            RequestedResources = new List<ResourceAmountDto>
            {
                new ResourceAmountDto { ResourceId = 3, Amount = 150 },
                new ResourceAmountDto { ResourceId = 4, Amount = 250 }
            }
        };

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Trade/CreateTradeAgreementWithResources", content);

        response.EnsureSuccessStatusCode();

        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostTrade_InvalidData()
    {
        var dto = new OfferTradeAgreementDTO
        {
            ReceivingNationId = 5,
            Duration = 2,
            Description = "Im description",
            TradeStatus = TradeStatus.Pending,
            OfferedResources = new List<ResourceAmountDto>{},
            RequestedResources = new List<ResourceAmountDto>{}
        };

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Trade/CreateTradeAgreementWithResources", content);

        XAssert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}