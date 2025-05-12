using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
using System.Linq;
using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;

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
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User not authenticated or invalid user ID"
                });
            }

            var gamesAccess = await _globalDbContext.GameAccesses
                .Where(g => g.UserId == userId)
                .Select(g => g.GameId) 
                .ToListAsync();

            if (gamesAccess.Count == 0)
            {
                return Ok(new List<GameDTO>());
            }

            var games = await _globalDbContext.Games
                .Where(g => gamesAccess.Contains((int)g.Id)) // Casting from nullable int (int?) to int because Game.Id is nullable
                .ToListAsync();

            var gamesDTOs = games.Select(game => new GameDTO(game.Id, game.Name, game.Description, game.Image)).ToList();

            return Ok(gamesDTOs);
        }

        [Authorize]
        [HttpPost("{gameId}/select")]
        public async Task<IActionResult> SelectGame(int gameId)
        {
            var game = await _globalDbContext.Games.FindAsync(gameId);

            if (game == null)
            {
                return NotFound("Game not found");
            }

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User not authenticated or invalid user ID"
                });
            }

            var access = await _globalDbContext.GameAccesses
                .Where(a => a.GameId == gameId && a.UserId == userId).FirstOrDefaultAsync();

            if (access == null) {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User is not game member"
                });
            }

            HttpContext.Session.SetString("Schema", game.Id.ToString());
            //var selectedGame = HttpContext.Session.GetString("SelectedGame");
            
            return Ok(new { selectedGameId = game.Id });
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

        [Authorize]
        [HttpPost("create-game")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameDTO creteGame)
        {

            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userClaimName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userClaimId == null || userClaimName == null)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "Missing user cookies"
                });
            }

            var userGames = await _globalDbContext.Games
                .Where(g => g.OwnerId == int.Parse(userClaimId))
                .ToListAsync();

            if (userGames.Count >= 2) // TODO: allow multiple games in the future
            {
                return Conflict(new
                {
                    error = "Conflict",
                    message = "User reached the maximum limit of owned games."
                });
            }

            var gameWithSameName = await _globalDbContext.Games
                .Where(g => g.Name == creteGame.Name)
                .FirstOrDefaultAsync();

            if (gameWithSameName != null) { 
                return Conflict(new
                {
                    error = "Conflict",
                    message = "Game with the same name already exists."
                });
            }

            var created_game = GameService.GenerateNewGame(
                "Host=localhost;Username=postgres;Password=postgres;Database=wg",
                 Path.Combine(Directory.GetCurrentDirectory(), "Migrations", "initate.sql"),
                 creteGame.Name    
            );

            if (!created_game)
            {
                return StatusCode(500, new
                {
                    error = "Internal Server Error",
                    message = "Failed to create game database."
                });
            }

            var newGame = new Game
            {
                Name = creteGame.Name,
                Description = creteGame.Description,
                Image = creteGame.Image,
                OwnerId = int.Parse(userClaimId)
            };

            _globalDbContext.Games.Add(newGame);
            await _globalDbContext.SaveChangesAsync();


            var gameAcces = new GameAccess
            {
                UserId = int.Parse(userClaimId),
                GameId = newGame.Id,
                Role = UserRole.GameMaster,
                IsArchived = false
            };

            _globalDbContext.GameAccesses.Add(gameAcces);
            await _globalDbContext.SaveChangesAsync();

            // return Created($"/games/{game.Id}", null);
            // TODO return something 
            return Created();
        }

    }
}
