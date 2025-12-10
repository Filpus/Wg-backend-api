using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Wg_backend_api.Data;
using Wg_backend_api.Services;

namespace Tests
{
internal class TestingWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string _schema = "game_1";
    private readonly string _nation = "1";
    private readonly Mock<ISessionDataService> _sessionDataService;


    public TestingWebAppFactory(string connectionString, string schema, string nation, Mock<ISessionDataService> sessionDataService)
    {
        _connectionString = connectionString;
        _schema = schema;
        _nation = nation;
        _sessionDataService = sessionDataService;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(_sessionDataService);
            services.RemoveAll<IGameDbContextFactory>();

            services.AddSingleton<IGameDbContextFactory>(sp =>
                new TestGameDbContextFactory(_connectionString));

            services.RemoveAll<ISessionDataService>();
            services.AddSingleton<ISessionDataService>(
                new TestSessionDataService(_schema, _nation, "Player"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
                .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>("Test", _ => { });

        });

        builder.UseEnvironment("Development");
    }
}
}
