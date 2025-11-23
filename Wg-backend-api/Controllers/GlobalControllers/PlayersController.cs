using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GlobalControllers
{
    [Authorize] // TODO fuszera drut
    [ApiController]
    [Route("api/games/[controller]")]

    public class PlayersController : ControllerBase
    {
        private readonly GlobalDbContext _globalDbContext;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int _userId;
        private int _gameId;

        public PlayersController(GlobalDbContext globalDb, IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this._globalDbContext = globalDb;
            this._sessionDataService = sessionDataService;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = gameDbFactory.Create(schema);
            this._gameId = schema != null ? int.Parse(schema.Replace("game_", string.Empty)) : -1;
            if (this._gameId == -1)
            {
                throw new InvalidOperationException("Nieprawidłowy identyfikator gry w schemacie.");
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetUserId(int userId)
        {
            this._userId = userId;
        }

        [HttpGet]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> GetPlayers()
        {
            var access = await this._globalDbContext.GameAccesses
                .FirstOrDefaultAsync(a => a.GameId == this._gameId && a.UserId == this._userId);

            if (access == null)
            {
                return Forbid();
            }

            var players = await this._context.Players.ToListAsync();

            return Ok(players);
        }
    }
}
