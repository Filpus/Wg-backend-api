using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers.GlobalControllers
{
    [ApiController]
    //[Route("api/games/{gameId}/[controller]")]
    [Route("api/games/[controller]")]

    public class PlayersController : ControllerBase
    {
        private readonly GlobalDbContext _globalDbContext;
        private readonly IGameDbContextFactory _gameDbContextFactory;

        public PlayersController(GlobalDbContext globalDb, IGameDbContextFactory gameDbFactory)
        {
            _globalDbContext = globalDb;
            _gameDbContextFactory = gameDbFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {

            var selectedGameStr = HttpContext.Session.GetString("Schema");

            if (string.IsNullOrEmpty(selectedGameStr) || !selectedGameStr.StartsWith("game_"))
            {
                return BadRequest("No game selected in session.");
            }

            var idPart = selectedGameStr.Replace("game_", "");
            if (!int.TryParse(idPart, out int gameId))
            {
                return BadRequest("Invalid game ID in session.");
            }

            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var user = await _globalDbContext.Users.FindAsync(ClaimTypes.NameIdentifier);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("User not authenticated or invalid user ID");
            }

            var access = await _globalDbContext.GameAccesses
                .FirstOrDefaultAsync(a => a.GameId == gameId && a.UserId == userId);
            
            if (access == null) {
                return Forbid();
            }

            var game = await _globalDbContext.Games.FindAsync(gameId);

            if (game == null)
            {
                return NotFound("Game not found");
            }
            
            var schema = $"game_{game.Name}";
            //HttpContext.Session.SetString("Schema", schema); //TO DO Tymczasowe ograniczenie!!!!!!!!!!!
            using var gameDb = _gameDbContextFactory.Create(schema);

            var players = await gameDb.Players.ToListAsync();

            return Ok(players);
        }


        /// <summary>
        /// Get selected game ID from session.
        /// </summary>
        /// <returns>Returns gameId otherwise null</returns>
        private int? GetSelectedGameId()
        {
            var selectedGameStr = HttpContext.Session.GetString("SelectedGame");
            if (int.TryParse(selectedGameStr, out int gameId))
                return gameId;

            return null;
        }
    }
}
