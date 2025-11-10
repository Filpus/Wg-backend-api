using Microsoft.EntityFrameworkCore;

namespace Wg_backend_api.Data
{
    public interface IGameDbContextFactory
    {
        public GameDbContext Create(string schema);
    }
    public class GameDbContextFactory : IGameDbContextFactory
    {
        private readonly DbContextOptions<GameDbContext> _options;

        public GameDbContextFactory(DbContextOptions<GameDbContext> options)
        {
            this._options = options;
        }

        public GameDbContext Create(string schema)
        {
            return new GameDbContext(this._options, schema);
        }
    }
}
