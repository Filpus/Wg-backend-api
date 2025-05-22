using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Armies")]
    [ApiController]
    public class ArmiesController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public ArmiesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        public async Task<ActionResult<IEnumerable<Army>>> GetArmies(int? id)
        {
            if (id.HasValue)
            {
                var army = await _context.Armies.FindAsync(id);
                if (army == null)
                {
                    return NotFound();
                }
                return Ok(new List<Army> { army });
            }
            else
            {
                return await _context.Armies.ToListAsync();
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutArmies([FromBody] List<Army> armies)
        {
            if (armies == null || armies.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var army in armies)
            {
                _context.Entry(army).State = EntityState.Modified;
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
        public async Task<ActionResult<Army>> PostArmies([FromBody] List<Army> armies)
        {
            if (armies == null || armies.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (Army army in armies)
            {
                army.Id = null;
            }

            _context.Armies.AddRange(armies);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArmies", new { id = armies[0].Id }, armies);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteArmies([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var armies = await _context.Armies.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (armies.Count == 0)
            {
                return NotFound("Nie znaleziono armii do usunięcia.");
            }

            _context.Armies.RemoveRange(armies);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
