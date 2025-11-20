using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Authorize] // TODO fuszera drut
    [ApiController]
    [Route("api/[controller]")]

    public class PlayersController : ControllerBase
    {
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private GlobalDbContext _globalDbContext;
        private int _gameId;

        public PlayersController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, GlobalDbContext globalDb)
        {
            this._sessionDataService = sessionDataService;
            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = gameDbFactory.Create(schema);
            this._gameId = schema != null ? int.Parse(schema.Replace("game_", string.Empty)) : -1;
            if (this._gameId == -1)
            {
                throw new InvalidOperationException("Nieprawidłowy identyfikator gry w schemacie.");
            }
            this._globalDbContext = globalDb;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            var players = await this._context.Players.ToListAsync();
            return Ok(players);
        }

        [HttpGet("with-nations")]
        public async Task<ActionResult<PlayerWithNationDTO[]>> GetPlayersWithNations()
        {
            var players = await this._context.Players
            .Select(p => new PlayerWithNationDTO
            {
                Id = (int)p.Id,
                Name = p.Name,
                Role = p.Role,
                Nation = p.Assignment != null ? new NationBaseInfoDTO
                {
                    Id = p.Assignment.Nation.Id,
                    Name = p.Assignment.Nation.Name,
                }
                : null,
            }).ToListAsync();
            return Ok(players);
        }

        // TODO ensure only GameMaster can access this endpoint
        // Or user is allowed to see unassigned players / acces to game is enough?
        [HttpGet("unassigned-players")]
        public async Task<ActionResult<PlayerDTO>> GetUnassignedPlayers()
        {
            var unassignedPlayers = await this._context.Players
                .Where(p => p.Assignment == null)
                .Select(p => new PlayerDTO
                {
                    Id = (int)p.Id,
                    Name = p.Name,
                    Role = p.Role,
                })
                .ToListAsync();

            return Ok(unassignedPlayers);
        }

        [HttpDelete]
        public async Task<ActionResult<PlayerDTO>> DeletePlayer([FromBody] int id)
        {
            var player = await this._context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            this._context.Players.Remove(player);
            await this._context.SaveChangesAsync();

            await this._globalDbContext.GameAccesses
                .Where(ga => ga.UserId == player.Id && ga.GameId == this._gameId)
                .ExecuteDeleteAsync();

            return this.Ok();
        }
    }
}
