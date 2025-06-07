using Bogus;
using Wg_backend_api.Models;
using BCrypt.Net;

namespace Wg_backend_api.Data.Seeders
{
    public class GlobalSeeder
    {
        private readonly GlobalDbContext _globalContext;
        public GlobalSeeder(GlobalDbContext globalContext)
        {
            _globalContext = globalContext;
        }

        public void Seed()
        {
            _globalContext.Database.EnsureCreated();
            if (_globalContext.GameAccesses.Any() || _globalContext.Games.Any() || _globalContext.Users.Any())
            {
                return;
            }


            _globalContext.Users.Add(new User {Name="admin", Email="admin@admin.pl", Password=BCrypt.Net.BCrypt.HashPassword("admin"), IsArchived=false });
            _globalContext.Users.AddRange(GetUserGenerator().Generate(50));
            _globalContext.SaveChanges();
            var usersId = _globalContext.Users.Select(u => u.Id).ToList();
            if (usersId == null || usersId.Count == 0)
            {
                throw new Exception("No users found in the database.");
            }
            var nonNullUserIds = usersId.Where(id => id.HasValue)
                            .Select(id => id.Value)
                            .ToList();

            _globalContext.Games.AddRange(GetGameGeneratorWithImages(nonNullUserIds).Generate(20));
            _globalContext.SaveChanges();

            var gamesId = _globalContext.Games.Select(g => g.Id).ToList();
            if (gamesId == null || gamesId.Count == 0)
            {
                throw new Exception("No games found in the database.");
            }
            _globalContext.GameAccesses.AddRange(GetGameAccessGenerator(nonNullUserIds, gamesId, 50));
            _globalContext.SaveChanges();
            

        }

        public Faker<User> GetUserGenerator()
        {
            return new Faker<User>().RuleFor(u => u.Name, f => f.Person.UserName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Password, f => BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()))
                .RuleFor(u => u.IsArchived, f => f.Random.Bool());
        }

        public Faker<Game> GetGameGeneratorWithImages(List<int> usersId)
        {

            return new Faker<Game>().RuleFor(g => g.Name, f => f.Commerce.ProductName())
                .RuleFor(g => g.Description, f => f.Lorem.Paragraph())
                .RuleFor(g => g.Image, f => null)
                .RuleFor(g => g.OwnerId, f => f.PickRandom(usersId));
        }

        private List<GameAccess> GetGameAccessGenerator(List<int> userIds, List<int> gameIds, int count)
        {
            var faker = new Faker();
            var result = new List<GameAccess>();
            var usedPairs = new HashSet<(int userId, int gameId)>();

            int maxCombinations = userIds.Count * gameIds.Count;
            count = Math.Min(count, maxCombinations);

            while (result.Count < count)
            {
                var userId = faker.PickRandom(userIds);
                var gameId = faker.PickRandom(gameIds);

                if (!usedPairs.Contains((userId, gameId)))
                {
                    usedPairs.Add((userId, gameId));

                    result.Add(new GameAccess
                    {
                        UserId = userId,
                        GameId = gameId,
                        Role = faker.PickRandom<UserRole>(),
                        IsArchived = false
                    });
                }
            }

            return result;
        }
    }
}
