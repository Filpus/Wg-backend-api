using Tests;
using Xunit;
using Wg_backend_api.Services;
using Moq;

public class PopulationsControllerTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestingWebAppFactory _factory;

    public PopulationsControllerTests()
    {
        string conn = TestDatabaseManager.RecreateDatabase();
        var mockSession = new Mock<ISessionDataService>();
        mockSession.Setup(s => s.GetNation()).Returns("1");
        mockSession.Setup(s => s.GetSchema()).Returns("game_1");
        mockSession.Setup(s => s.GetRole()).Returns("Player");

        _factory = new TestingWebAppFactory(conn, schema: "game_1", nation: "1",mockSession);
        _client = _factory.CreateClient();

    }

    [Fact]
    public async Task GetAll_ReturnsOkAndData()
    {
        var response = await _client.GetAsync("/api/auth/status");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        Console.WriteLine(json);

        var responsepop = await _client.GetAsync("/api/populations");
        responsepop.EnsureSuccessStatusCode();

        string jsonpop = await responsepop.Content.ReadAsStringAsync();
        Console.WriteLine(jsonpop);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
        TestDatabaseManager.DropDatabase();
    }
}

