using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GlobalControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly GlobalDbContext _globalDbContext;
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;

        public GamesController(GlobalDbContext globalDb, IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            _globalDbContext = globalDb;
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;

        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetGames()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (!int.TryParse(userIdStr, out int userId))
            {
                return NotFound("User not found");
            }

            var gamesAccess = await _globalDbContext.GameAccesses
                .Where(g => g.UserId == userId)
                .ToListAsync();

 
            if (gamesAccess == null || gamesAccess.Count == 0)
            {
                return NotFound("No games found for this user");
            }

            var games = await _globalDbContext.Games.ToListAsync();
            var gamesDTOs = gamesAccess.
                Join(games, 
                access => access.GameId, 
                game=> game.Id,
                (access, game) => new GameDTO(game.Id, game.Name, game.Description, game.Image)
                ).ToList();

            
            return Ok(new PlayerGamesDTO().PlayerGames = gamesDTOs);
        }

        [Authorize]
        [HttpPost("select-game")]
        public async Task<IActionResult> SelectGame([FromBody] int gameId)
        {
            var game = await _globalDbContext.Games.FindAsync(gameId);

            if (game == null)
            {
                return NotFound("Game not found");
            }

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
            {
                return NotFound("User not found");
            }

            var access = await _globalDbContext.GameAccesses
                .Where(a => a.GameId == gameId && a.UserId == userId).FirstOrDefaultAsync();

            if (access == null) { 
                return Unauthorized("User is not game member");
            }

            _sessionDataService.SetSchema(game.Name);
            //HttpContext.Session.SetString("Schema", game.Id.ToString());
            //var selectedGame = HttpContext.Session.GetString("SelectedGame");

            
            return Ok(game.Id);
        }

        [Authorize]
        [HttpGet("get-session-schema")]
        public IActionResult GetSessionSchema()
        {
            var schemaValue = HttpContext.Session.GetString("Schema");

            if (schemaValue != null)
            {
                return Ok(new { Schema = schemaValue });
            }
            else
            {
                return NotFound(new { Message = "Session value not found" });
            }
        }

    }
}
