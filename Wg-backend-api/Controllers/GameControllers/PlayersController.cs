using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Security.Claims;
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
        private int _gameId;

        public PlayersController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            var players = await this._context.Players.ToListAsync();
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
    }
}
