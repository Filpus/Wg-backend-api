using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wg_backend_api.Data;

namespace Wg_backend_api.SecControllers
{
    [Authorize]
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private readonly GlobalDbContext _context;

        public GamesController(GlobalDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-games")]
        public async Task<IActionResult> GetMyGames()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);  

            //var games = await _context.GameAccesses
            //    .Where(ug => ug.UserId == userId)
            //    .Select(ug => new {ug.GameId})
            //    .ToListAsync();

            //return Ok(games);
            return Ok();
        }
    }



}
