using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
using System;
using System.Linq;
using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GlobalControllers
{
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
                .Where(g => gamesAccess.Contains((int)g.Id))
                .ToListAsync();

            var gamesDTOs = games.Select(game => new GameDTO(game.Id, game.Name, game.Description, game.Image)).ToList();

            return Ok(gamesDTOs);
        }



        [HttpPost("select")]
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

            var gameDbContext = _gameDbContextFactory.Create($"game_{game.Id}");
            
            var userInGame = await gameDbContext.Players.Where(u => u.UserId == userId).FirstOrDefaultAsync();
            if (userInGame == null)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User is not game member"
                });
            }
            var accesToNation = await gameDbContext.Assignments
                .Include(a => a.Nation)
                .Where(a => a.UserId == userId && a.IsActive)
                .FirstOrDefaultAsync();
            if (accesToNation == null)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User is not game member"
                });
            }

            _sessionDataService.SetNation($"{accesToNation.Nation.Id}");
            _sessionDataService.SetSchema($"{game.Name}");
            
            return Ok(new { selectedGameId = game.Id });
        }


        [HttpGet("get-session-schema")]
        public IActionResult GetSessionSchema()
        {
            var schemaValue = _sessionDataService.GetSchema();

            if (schemaValue != null)
            {
                return Ok(new { Schema = schemaValue });
            }
            else
            {
                return NotFound(new { Message = "Session value not found" });
            }
        }

        [HttpGet("get-session-nation")]
        public IActionResult GetSessionNation()
        {
            var schemaValue = _sessionDataService.GetNation();

            if (schemaValue != null)
            {
                return Ok(new { Nation = schemaValue });
            }
            else
            {
                return NotFound(new { Message = "Session value not found" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGame([FromForm] CreateGameDTO creteGame)
        {

            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userClaimName = User.FindFirst(ClaimTypes.Name)?.Value;

            // TODO remove after enable middleware
            if (userClaimId == null || userClaimName == null)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User not authenticated or invalid user ID"
                });
            }

            var userGames = await _globalDbContext.Games
                .Where(g => g.OwnerId == int.Parse(userClaimId))
                .ToListAsync();

            if (userGames.Count >= 2 && userClaimName != "admin") // TODO: allow multiple games in the future
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

            var gameImagePath = "";

            if (creteGame.ImageFile != null) { 
                try
                {
                    if (creteGame.ImageFile.Length == 0) { 
                        return BadRequest("Plik obrazu jest pusty lub nie został przesłany.");
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var fileExtension = Path.GetExtension(creteGame.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest($"Nieobsługiwany format pliku. Dopuszczalne rozszerzenia: {string.Join(", ", allowedExtensions)}");

                    const int maxFileSize = 20 * 1024 * 1024; // 5 MB
                    if (creteGame.ImageFile.Length > maxFileSize)
                        return BadRequest($"Maksymalny dopuszczalny rozmiar pliku to {maxFileSize / 1024 / 1024} MB");

                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await creteGame.ImageFile.CopyToAsync(stream);
                    }

                    gameImagePath = $"/images/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        $"Wystąpił błąd podczas przetwarzania pliku: {ex.Message}"
                    );
                }
            }

            var newGame = new Game
            {
                Name = creteGame.Name,
                Description = creteGame.Description,
                Image = gameImagePath != "" ? gameImagePath : null,
                OwnerId = int.Parse(userClaimId)
            };

            _globalDbContext.Games.Add(newGame);
            await _globalDbContext.SaveChangesAsync();

            var created_game = GameService.GenerateNewGame(
                "Host=localhost;Username=postgres;Password=postgres;Database=wg",
                 Path.Combine(Directory.GetCurrentDirectory(), "Migrations", "initate.sql"),
                 $"game_{newGame.Id}"
            );

            if (!created_game)
            {
                _globalDbContext.Games.Remove(newGame);
                await _globalDbContext.SaveChangesAsync();

                return StatusCode(500, new
                {
                    error = "Internal Server Error",
                    message = "Failed to create game database."
                });
            }

            var gameAcces = new GameAccess
            {
                UserId = int.Parse(userClaimId),
                GameId = newGame.Id,
                Role = UserRole.GameMaster,
                IsArchived = false
            };

            _globalDbContext.GameAccesses.Add(gameAcces);
            await _globalDbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Game created successfully",
                game = new GameDTO(newGame.Id, newGame.Name, newGame.Description, newGame.Image)
            });
        }

    }
}
