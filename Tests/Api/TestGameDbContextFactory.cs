using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;

namespace Tests.Api;

public class TestGameDbContextFactory : IGameDbContextFactory
{
    private readonly string _connectionString;

    public TestGameDbContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public GameDbContext Create(string schema)
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new GameDbContext(options, schema);
    }
}
