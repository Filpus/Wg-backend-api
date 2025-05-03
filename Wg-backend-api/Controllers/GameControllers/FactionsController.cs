using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Factions")]
    [ApiController]
    public class FactionsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;

        public FactionsController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;
            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Faction>>> GetFactions(int? id)
        {
            if (id.HasValue)
            {
                var faction = await _context.Factions.FindAsync(id);
                if (faction == null)
                {
                    return NotFound();
                }
                return Ok(new List<Faction> { faction });
            }
            else
            {
                return await _context.Factions.ToListAsync();
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutFactions([FromBody] List<Faction> factions)
        {
            if (factions == null || factions.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var faction in factions)
            {
                _context.Entry(faction).State = EntityState.Modified;
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
        public async Task<ActionResult<Faction>> PostFactions([FromBody] List<Faction> factions)
        {
            if (factions == null || factions.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (Faction faction in factions)
            {
                faction.Id = null;
            }

            _context.Factions.AddRange(factions);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFactions", new { id = factions[0].Id }, factions);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteFactions([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var factions = await _context.Factions.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (factions.Count == 0)
            {
                return NotFound("Nie znaleziono frakcji do usunięcia.");
            }

            _context.Factions.RemoveRange(factions);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("ByNation/{nationId}")]
        public async Task<ActionResult<IEnumerable<Faction>>> GetFactionsByNation(int nationId)
        {
            var factions = await _context.Factions.Where(f => f.NationId == nationId).ToListAsync();

            if (factions == null || factions.Count == 0)
            {
                return NotFound($"Nie znaleziono frakcji dla państwa o ID {nationId}.");
            }

            return Ok(factions);
        }
    }
}
