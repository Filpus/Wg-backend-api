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
    [Route("api/[controller]")]
    public class PlayersController : GameControllerBase
    {
        private readonly GlobalDbContext _globalDbContext;
        private readonly int _gameId;

        public PlayersController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, GlobalDbContext globalDb)
            : base(gameDbFactory, sessionDataService)
        {
            this._globalDbContext = globalDb;
            string schema = this.SessionDataService.GetSchema();
            this._gameId = int.Parse(schema.Replace("game_", string.Empty));
            if (this._gameId == -1)
            {
                throw new InvalidOperationException("Nieprawidłowy identyfikator gry w schemacie.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            var players = await this.Context.Players.ToListAsync();
            return Ok(players);
        }

        [HttpGet("with-nations")]
        public async Task<ActionResult<PlayerWithNationDTO[]>> GetPlayersWithNations()
        {
            var players = await this.Context.Players
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
            var unassignedPlayers = await this.Context.Players
                .Where(p => p.Assignment == null && p.Role == UserRole.Player)
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
            var player = await this.Context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            if (player.Role == UserRole.GameMaster)
            {
                return BadRequest("Cannot delete GameMaster player.");
            }

            this.Context.Players.Remove(player);
            await this.Context.SaveChangesAsync();

            await this._globalDbContext.GameAccesses
                .Where(ga => ga.UserId == player.Id && ga.GameId == this._gameId)
                .ExecuteDeleteAsync();

            return this.Ok();
        }
    }
}
