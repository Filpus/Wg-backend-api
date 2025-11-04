using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    public class PlayerController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private GlobalDbContext _globalDbContext;
        public PlayerController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, GlobalDbContext globalDbContext)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;
            _globalDbContext = globalDbContext;

            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }
            _context = _gameDbContextFactory.Create(schema);
        }
        [HttpGet]
        [Route("GetAllPlayers")]
        public async Task<ActionResult<IEnumerable<PlayerInfoDTO>>> GetAllPlayers()
        {
            var users = await _globalDbContext.Users.ToListAsync();
            var players = await _context.Players
                .Include(p => p.Assignment)
                .ThenInclude(a => a.Nation)
                .ToListAsync();

            var playerInfoList = players.Select(p =>
            {
                var user = users.FirstOrDefault(u => u.Id == p.UserId);
                return new PlayerInfoDTO
                {
                    Name = p.Name,
                    NationName = p.Assignment?.Nation?.Name,
                    userRole = p.Role
                };
            }).ToList();

            return Ok(playerInfoList);
        }
        
    }
}
