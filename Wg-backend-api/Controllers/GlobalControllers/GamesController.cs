using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly GameService _gameService;
        private int _userId;

        public GamesController(GlobalDbContext globalDb, IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, GameService gameService)
        {
            this._globalDbContext = globalDb;
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;
            this._gameService = gameService;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetUserId(int userId)
        {
            this._userId = userId;
        }

        [HttpGet]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> GetGames()
        {
            var gamesAccess = await this._globalDbContext.GameAccesses
                .Where(g => g.UserId == this._userId)
                .Select(g => new GameDTO(g.Game.Id, g.Game.Name, g.Game.Description, g.Game.Image, g.Game.GameCode))
                .ToListAsync();

            return this.Ok(gamesAccess);
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> GetSpecificGame(int id)
        {
            var gamesAccess = await this._globalDbContext.GameAccesses
                .Where(g => g.UserId == this._userId && g.GameId == id)
                .Select(g => new GameDTO(g.Game.Id, g.Game.Name, g.Game.Description, g.Game.Image, g.Game.GameCode))
                .ToListAsync();

            if (!gamesAccess.Any())
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = $"Game with ID {id} not found or user have no access"
                });
            }

            return this.Ok(gamesAccess);
        }

        [HttpGet("selected")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> GetSelectedGame()
        {
            var selectedGame = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(selectedGame) || !selectedGame.StartsWith("game_"))
            {
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = "No game selected in session",
                });
            }

            var gameId = int.Parse(selectedGame.Split('_')[1]);

            var gamesAccess = await this._globalDbContext.GameAccesses
                .Where(g => g.UserId == this._userId && g.GameId == gameId)
                .Select(g => new GameDTO(g.Game.Id, g.Game.Name, g.Game.Description, g.Game.Image, g.Game.GameCode))
                .ToListAsync();

            if (!gamesAccess.Any())
            {
                return Ok();
            }

            return this.Ok(gamesAccess.First());
        }

        [HttpGet("with-roles")]
        public async Task<IActionResult> GetGamesWithRoles()
        {
            var gamesAccess = await this._globalDbContext.GameAccesses
                .Where(g => g.UserId == this._userId)
                .Select(g => new GameWithRoleDTO(g.Game.Id, g.Game.Name, g.Game.Description, g.Game.Image, g.Game.GameCode, g.Role))
                .ToListAsync();

            return this.Ok(gamesAccess);
        }

        [HttpGet("detailed/{gameId}")]
        public async Task<IActionResult> GetGameDetail(int gameId)
        {
            var gamesAccess = await this._globalDbContext.GameAccesses
                .Where(g => g.UserId == this._userId && g.GameId == gameId)
                .ToListAsync();

            if (!gamesAccess.Any())
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = $"Game with ID {gameId} not found or user have no access"
                });
            }

            var gameContext = this._gameDbContextFactory.Create($"game_{gameId}");

            var gameDetails = await gameContext.Assignments
                .Include(a => a.Nation)
                .Where(a => a.IsActive && a.UserId == this._userId)
                .ToListAsync();

            var playerCount = await gameContext.Players.CountAsync();

            return Ok(new GameInfoDTO
            {
                OwnedNationName = gameDetails.FirstOrDefault()?.Nation.Name,
                CurrentUsers = playerCount,
                Role = gamesAccess.First().Role,
            });
        }

        [HttpPost("joinGame")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> JoinGame([FromForm] string gameCode)
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
        [ServiceFilter(typeof(UserIdActionFilter))]
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
            if (userInGame.Role != UserRole.GameMaster)
            {
                var accesToNation = await gameDbContext.Assignments
                    .Include(a => a.Nation)
                    .Where(a => a.UserId == userInGame.Id)
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
            }
            else
            {
                // TODO Gm cannot have assigned nation !!!!!!
                // TODO temporary walkaround -1 nation for gm !!!!!!!!
                this._sessionDataService.SetNation($"{-1}");
            }

            this._sessionDataService.SetSchema($"game_{game.Id}");
            this._sessionDataService.SetRole($"{access.Role}");

            return Ok(new { selectedGameId = game.Id, roleInGame = access.Role });
        }

        [HttpPost("select-nation")]
        public async Task<IActionResult> SelectNation([FromBody] int nationId)
        {
            var schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = "No game selected in session",
                });
            }

            var gameDbContext = this._gameDbContextFactory.Create(schema);
            var nation = await gameDbContext.Nations.FindAsync(nationId);
            if (nation == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = "Nation not found in the selected game",
                });
            }

            this._sessionDataService.SetNation($"{nation.Id}");
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
        [ServiceFilter(typeof(UserIdActionFilter))]
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
                var result = this.UploadGameImage(creteGame.ImageFile, null);
                if (!result.Result.success)
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = result.Result.errorMessage,
                    });
                }

                gameImagePath = result.Result.imagePath;
            }

            var newGame = new Game
            {
                Name = creteGame.Name,
                Description = creteGame.Description,
                Image = gameImagePath != string.Empty ? gameImagePath : null,
                OwnerId = this._userId,
            };

            this._globalDbContext.Games.Add(newGame);
            await this._globalDbContext.SaveChangesAsync();

            var created_game = this._gameService.GenerateNewGame(
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

            var newGameDbContext = this._gameDbContextFactory.Create($"game_{newGame.Id}");
            var gameMasterPlayer = new Player
            {
                UserId = this._userId,
                Name = userName,
                Role = UserRole.GameMaster,
            };
            newGameDbContext.Players.Add(gameMasterPlayer);
            await newGameDbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Game created successfully",
                game = new GameDTO(newGame.Id, newGame.Name, newGame.Description, newGame.Image, newGame.GameCode),
            });
        }

        [HttpDelete("removePlayer/{gameId}/{userId}")]
        [ServiceFilter(typeof(UserIdActionFilter))]
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

        [HttpDelete("leave")]
        public async Task<IActionResult> RemovePlayer([FromBody] int gameId)
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

            if (access == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = "User does not have access to this game",
                });
            }

            if (access.Role == UserRole.GameMaster)
            {
                // I'm not sure if we should check if gm is game owner
                this._gameService.DeleteGameSchema($"game_{gameId}");
                this._globalDbContext.GameAccesses.RemoveRange(
                    this._globalDbContext.GameAccesses.Where(a => a.GameId == gameId)
                );

                this._globalDbContext.Games.Remove(game);
                await this._globalDbContext.SaveChangesAsync();
            }
            else if (access.Role == UserRole.Player)
            {
                var gameDbContext = this._gameDbContextFactory.Create($"game_{gameId}");
                var player = await gameDbContext.Players
                    .Where(p => p.UserId == this._userId)
                    .FirstOrDefaultAsync();

                if (player != null)
                {
                    gameDbContext.Players.Remove(player);
                    await gameDbContext.SaveChangesAsync();
                }
                // TODO ensure assigmnet is deactivated

                this._globalDbContext.GameAccesses.Remove(access);
                await this._globalDbContext.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Player successfully removed from the game",
            });
        }

        [HttpPut]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> UpdateGame([FromForm] PutGameDTO updateGame)
        {
            var game = await this._globalDbContext.Games.Where(g => g.OwnerId == this._userId && g.Id == updateGame.Id).FirstOrDefaultAsync();
            if (game == null)
            {
                return NotFound(new
                {
                    error = "Not Found",
                    message = "Game not found or user is not the owner",
                });
            }

            game.Name = updateGame.Name ?? game.Name;
            game.Description = updateGame.Description ?? game.Description;
            game.GameCode = updateGame.GameCode ?? game.GameCode;

            if (updateGame.ImageFile != null)
            {
                var (success, imagePath, errorMessage) = await this.UploadGameImage(updateGame.ImageFile, game.Image);
                if (!success)
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = errorMessage,
                    });
                }

                game.Image = imagePath;
            }

            this._globalDbContext.Games.Update(game);
            await this._globalDbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Game updated successfully",
            });
        }

        private async Task<(bool success, string imagePath, string errorMessage)> UploadGameImage(IFormFile imageFile, string? oldImagePath)
        {
            var gameImagePath = string.Empty;

            if (imageFile != null)
            {
                try
                {
                    if (imageFile.Length == 0)
                    {
                        return (false, string.Empty, "Plik obrazu jest pusty lub nie został przesłany.");
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return (false, string.Empty, $"Nieobsługiwany format pliku. Dopuszczalne rozszerzenia: {string.Join(", ", allowedExtensions)}");
                    }

                    const int maxFileSize = 5 * 1024 * 1024; // 5 MB
                    if (imageFile.Length > maxFileSize)
                    {
                        return (false, string.Empty, $"Maksymalny dopuszczalny rozmiar pliku to {maxFileSize / 1024 / 1024} MB");
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
                        await imageFile.CopyToAsync(stream);
                    }

                    gameImagePath = $"/images/{uniqueFileName}";

                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        var oldFileName = Path.GetFileName(oldImagePath);
                        var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return (false, string.Empty, $"Wystąpił błąd podczas przetwarzania pliku: {ex.Message}");
                }
            }

            return (true, gameImagePath, string.Empty);
        }
    }
}
