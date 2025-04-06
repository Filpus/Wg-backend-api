using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/games/{gameId}/[controller]")]
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
        public async Task<IActionResult> GetPlayers(int gameId)
        {


            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var user = await _globalDbContext.Users.FindAsync(ClaimTypes.NameIdentifier);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }


            var access = await _globalDbContext.GameAccesses
                .FirstOrDefaultAsync(a => a.GameId == gameId && a.UserId == userId);
            Console.WriteLine($"Access: {access}");
            if (access == null) {
                return Forbid();
            }

            var game = await _globalDbContext.Games.FindAsync(gameId);

            if (game == null)
            {
                return NotFound("Game not found");
            }

            var schema = game.Name;
            using var gameDb = _gameDbContextFactory.Create(schema);

            var players = await gameDb.Players.ToListAsync();

            return Ok(players);
        }
    }
}
