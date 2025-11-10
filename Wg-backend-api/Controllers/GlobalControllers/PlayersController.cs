using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GlobalControllers
{
    [ApiController]
    [Route("api/games/[controller]")]

    public class PlayersController : ControllerBase
    {
        private readonly GlobalDbContext _globalDbContext;
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private int _userId;

        public PlayersController(GlobalDbContext globalDb, IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this._globalDbContext = globalDb;
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetUserId(int userId)
        {
            this._userId = userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            var selectedGameStr = this._sessionDataService.GetSchema();

            var idPart = selectedGameStr.Replace("game_", string.Empty);
            if (!int.TryParse(idPart, out int gameId))
            {
                return BadRequest("Invalid game ID in session.");
            }

            var access = await this._globalDbContext.GameAccesses
                .FirstOrDefaultAsync(a => a.GameId == gameId && a.UserId == this._userId);

            if (access == null)
            {
                return Forbid();
            }

            var game = await this._globalDbContext.Games.FindAsync(gameId);

            if (game == null)
            {
                return NotFound("Game not found");
            }

            var schema = $"game_{game.Name}";
            using var gameDb = this._gameDbContextFactory.Create(schema);

            var players = await gameDb.Players.ToListAsync();

            return Ok(players);
        }
    }
}
