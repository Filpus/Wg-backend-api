using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/MaintenaceCosts")]
    [ApiController]
    public class MaintenaceCostsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public MaintenaceCostsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;

            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }
            _context = _gameDbContextFactory.Create(schema);
        }
        [HttpGet("{id?}")]


        [HttpDelete]
        public async Task<ActionResult> DeleteMaintenaceCosts([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var maintenaceCosts = await _context.MaintenaceCosts.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (maintenaceCosts.Count == 0)
            {
                return NotFound("Nie znaleziono kosztów utrzymania do usunięcia.");
            }

            _context.MaintenaceCosts.RemoveRange(maintenaceCosts);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
