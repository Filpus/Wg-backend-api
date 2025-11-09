using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
using System;
using System.Linq;
using System.Security.Claims;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GlobalControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : Controller
    {
        private readonly GlobalDbContext _globalDbContext;
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private int _userId;

        public GamesController(GlobalDbContext globalDb, IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this._globalDbContext = globalDb;
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetUserId(int userId)
        {
            _userId = userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetGames()
        {
            Console.WriteLine($"UserId in GamesController: {this._userId}");
            var gamesAccess = await this._globalDbContext.GameAccesses
                .Where(g => g.UserId == this._userId)
                .Select(g => g.GameId)
                .ToListAsync();

            if (gamesAccess.Count == 0)
            {
                return this.Ok(new List<GameDTO>());
            }

            var games = await this._globalDbContext.Games
                .Where(g => gamesAccess.Contains((int)g.Id))
                .ToListAsync();

            var gamesDTOs = games.Select(game => new GameDTO(game.Id, game.Name, game.Description, game.Image, game.GameCode)).ToList();

            return this.Ok(gamesDTOs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecificGame(int id)
        {
            var hasAccess = await this._globalDbContext.GameAccesses
                .AnyAsync(g => g.UserId == this._userId && g.GameId == id);

            if (!hasAccess)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = $"Game with ID {id} not found"
                });
            }

            var game = await _globalDbContext.Games
                .Where(g => g.Id == id)
                .Select(g => new GameDTO(g.Id, g.Name, g.Description, g.Image, g.GameCode))
                .FirstOrDefaultAsync();

            if (game == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = $"Game with ID {id} not found"
                });
            }

            return Ok(game);
        }

        [HttpPost("joinGame")]
        public async Task<IActionResult> JoinGame([FromBody] string gameCode)
        {
            var game = await this._globalDbContext.Games
                .Where(g => g.GameCode == gameCode)
                .FirstOrDefaultAsync();
            if (game == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = "Game with the provided code does not exist.",
                });
            }

            var existingAccess = await this._globalDbContext.GameAccesses
                .Where(a => a.GameId == game.Id && a.UserId == this._userId)
                .FirstOrDefaultAsync();
            if (existingAccess != null)
            {
                return Conflict(new
                {
                    error = "Conflict",
                    message = "User already has access to this game.",
                });
            }

            var gameAccess = new GameAccess
            {
                GameId = game.Id,
                UserId = this._userId,
                Role = UserRole.Player,
                IsArchived = false,
            };
            this._globalDbContext.GameAccesses.Add(gameAccess);
            await this._globalDbContext.SaveChangesAsync();

            var user = await this._globalDbContext.Users.FindAsync(this._userId);
            if (user == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = "User not found.",
                });
            }

            var gameDbContext = this._gameDbContextFactory.Create($"game_{game.Id}");
            var player = new Player
            {
                UserId = this._userId,
                Role = UserRole.Player,
                Name = user.Name,
            };
            gameDbContext.Players.Add(player);
            await gameDbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Successfully joined the game.",
                game = new GameDTO(game.Id, game.Name, game.Description, game.Image, game.GameCode),
            });
        }

        [HttpPost("select")]
        public async Task<IActionResult> SelectGame([FromBody] int gameId)
        {
            var game = await this._globalDbContext.Games.FindAsync(gameId);

            if (game == null)
            {
                return NotFound("Game not found");
            }

            var access = await this._globalDbContext.GameAccesses
                .Where(a => a.GameId == gameId && a.UserId == this._userId).FirstOrDefaultAsync();

            if (access == null)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User is not game member",
                });
            }

            var gameDbContext = this._gameDbContextFactory.Create($"game_{game.Id}");

            var userInGame = await gameDbContext.Players.Where(u => u.UserId == this._userId).FirstOrDefaultAsync();
            if (userInGame == null)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User is not game member",
                });
            }

            // TODO ensure gm has nation assigned / can select game
            var accesToNation = await gameDbContext.Assignments
                .Include(a => a.Nation)
                .Where(a => a.UserId == this._userId && a.IsActive)
                .FirstOrDefaultAsync();
            if (accesToNation == null)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User is not game member",
                });
            }

            this._sessionDataService.SetNation($"{accesToNation.Nation.Id}");
            this._sessionDataService.SetSchema($"game_{game.Id}");
            this._sessionDataService.SetRole($"{access.Role}");

            return Ok(new { selectedGameId = game.Id, roleInGame = access.Role });
        }

        [HttpPost("select-nation")]
        public async Task<IActionResult> SelectNation([FromBody] int nationId)
        {
            var schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = "No game selected in session"
                });
            }
            var gameDbContext = _gameDbContextFactory.Create(schema);
            var nation = await gameDbContext.Nations.FindAsync(nationId);
            if (nation == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = "Nation not found in the selected game"
                });
            }
            _sessionDataService.SetNation($"{nation.Id}");
            return Ok(new { selectedNationId = nation.Id });
        }   

        [HttpGet("get-session-schema")]
        public IActionResult GetSessionSchema()
        {
            var schemaValue = this._sessionDataService.GetSchema();

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
            var schemaValue = this._sessionDataService.GetNation();

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
            var userGames = await this._globalDbContext.Games
                .Where(g => g.OwnerId == this._userId)
                .ToListAsync();

            var userName = await this._globalDbContext.Users
                .Where(u => u.Id == this._userId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();

            if (userName == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = "User not found.",
                });
            }

            if (userGames.Count >= 1 && userName != "admin") // TODO: allow multiple games in the future
            {
                return Conflict(new
                {
                    error = "Conflict",
                    message = "User reached the maximum limit of owned games.",
                });
            }

            var gameWithSameName = await this._globalDbContext.Games
                .Where(g => g.Name == creteGame.Name)
                .FirstOrDefaultAsync();

            if (gameWithSameName != null)
            {
                return Conflict(new
                {
                    error = "Conflict",
                    message = "Game with the same name already exists.",
                });
            }

            var gameImagePath = string.Empty;

            if (creteGame.ImageFile != null)
            {
                try
                {
                    if (creteGame.ImageFile.Length == 0)
                    {
                        return BadRequest("Plik obrazu jest pusty lub nie został przesłany.");
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var fileExtension = Path.GetExtension(creteGame.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest($"Nieobsługiwany format pliku. Dopuszczalne rozszerzenia: {string.Join(", ", allowedExtensions)}");
                    }

                    const int maxFileSize = 20 * 1024 * 1024; // 5 MB
                    if (creteGame.ImageFile.Length > maxFileSize)
                    {
                        return BadRequest($"Maksymalny dopuszczalny rozmiar pliku to {maxFileSize / 1024 / 1024} MB");
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

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
                Image = gameImagePath != string.Empty ? gameImagePath : null,
                OwnerId = this._userId,
            };

            _globalDbContext.Games.Add(newGame);
            await _globalDbContext.SaveChangesAsync();

            var created_game = GameService.GenerateNewGame(
                "Host=localhost;Username=postgres;Password=postgres;Database=wg", //TODO Niezapomnieć że trzeba to będzie poprawnie ustawić
                Path.Combine(Directory.GetCurrentDirectory(), "Migrations", "game-schema-init.sql"),
                $"game_{newGame.Id}"
            );

            if (!created_game)
            {
                this._globalDbContext.Games.Remove(newGame);
                await this._globalDbContext.SaveChangesAsync();

                return StatusCode(500, new
                {
                    error = "Internal Server Error",
                    message = "Failed to create game database.",
                });
            }

            var gameAcces = new GameAccess
            {
                UserId = this._userId,
                GameId = newGame.Id,
                Role = UserRole.GameMaster,
                IsArchived = false,
            };

            this._globalDbContext.GameAccesses.Add(gameAcces);
            await this._globalDbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Game created successfully",
                game = new GameDTO(newGame.Id, newGame.Name, newGame.Description, newGame.Image, newGame.GameCode),
            });
        }

        [HttpDelete("removePlayer/{gameId}/{userId}")]
        public async Task<IActionResult> RemovePlayer(int gameId, int userId)
        {
            var game = await this._globalDbContext.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = "Game not found",
                });
            }

            var access = await this._globalDbContext.GameAccesses
                .Where(a => a.GameId == gameId && a.UserId == this._userId)
                .FirstOrDefaultAsync();

            if (access == null || access.Role != UserRole.GameMaster)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "Only the Game Master can remove players from the game",
                });
            }

            var playerAccess = await this._globalDbContext.GameAccesses
                .Where(a => a.GameId == gameId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (playerAccess == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = "Player does not have access to this game",
                });
            }

            this._globalDbContext.GameAccesses.Remove(playerAccess);
            await this._globalDbContext.SaveChangesAsync();

            var gameDbContext = this._gameDbContextFactory.Create($"game_{gameId}");
            var player = await gameDbContext.Players
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync();

            if (player != null)
            {
                gameDbContext.Players.Remove(player);
                await gameDbContext.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Player successfully removed from the game",
            });
        }
    }
}
