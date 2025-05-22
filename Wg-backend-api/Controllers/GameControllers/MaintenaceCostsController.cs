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
        public async Task<ActionResult<IEnumerable<MaintenaceCosts>>> GetMaintenaceCosts(int? id)
        {
            if (id.HasValue)
            {
                var maintenaceCost = await _context.MaintenaceCosts.FindAsync(id);
                if (maintenaceCost == null)
                {
                    return NotFound();
                }
                return Ok(new List<MaintenaceCosts> { maintenaceCost });
            }
            else
            {
                return await _context.MaintenaceCosts.ToListAsync();
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutMaintenaceCosts([FromBody] List<MaintenaceCosts> maintenaceCosts)
        {
            if (maintenaceCosts == null || maintenaceCosts.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var maintenaceCost in maintenaceCosts)
            {
                _context.Entry(maintenaceCost).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<MaintenaceCosts>> PostMaintenaceCosts([FromBody] List<MaintenaceCosts> maintenaceCosts)
        {
            if (maintenaceCosts == null || maintenaceCosts.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (MaintenaceCosts maintenaceCost in maintenaceCosts)
            {
                maintenaceCost.Id = null;
            }

            _context.MaintenaceCosts.AddRange(maintenaceCosts);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMaintenaceCosts", new { id = maintenaceCosts[0].Id }, maintenaceCosts);
        }

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
