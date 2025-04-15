using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wg_backend_api.Data;

namespace Wg_backend_api.Controllers.GlobalControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly GlobalDbContext _globalDbContext;
        private readonly IGameDbContextFactory _gameDbContextFactory;

        public GamesController(GlobalDbContext globalDb, IGameDbContextFactory gameDbFactory)
        {
            _globalDbContext = globalDb;
            _gameDbContextFactory = gameDbFactory;
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

            var games = await _globalDbContext.GameAccesses
                .Where(g => g.UserId == userId)
                .ToListAsync();

  
            if (games == null || games.Count == 0)
            {
                return NotFound("No games found for this user");
            }

            return Ok(games);
        }
    }
}
