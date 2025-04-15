using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Troops")]
    [ApiController]
    public class TroopsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;

        public TroopsController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;

            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        // GET: api/Troops
        // GET: api/Troops/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Troop>>> GetTroops(int? id)
        {
            if (id.HasValue)
            {
                var troop = await _context.Troops.FindAsync(id);
                if (troop == null)
                {
                    return NotFound();
                }
                return Ok(new List<Troop> { troop });
            }
            else
            {
                return await _context.Troops.ToListAsync();
            }
        }

        // PUT: api/Troops
        [HttpPut]
        public async Task<IActionResult> PutTroops([FromBody] List<Troop> troops)
        {
            if (troops == null || troops.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var troop in troops)
            {
                _context.Entry(troop).State = EntityState.Modified;
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

        // POST: api/Troops
        [HttpPost]
        public async Task<ActionResult<Troop>> PostTroops([FromBody] List<Troop> troops)
        {
            if (troops == null || troops.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (Troop troop in troops)
            {
                troop.Id = null;
            }

            _context.Troops.AddRange(troops);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTroops", new { id = troops[0].Id }, troops);
        }

        // DELETE: api/Troops
        [HttpDelete]
        public async Task<ActionResult> DeleteTroops([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var troops = await _context.Troops.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (troops.Count == 0)
            {
                return NotFound("Nie znaleziono jednostek do usunięcia.");
            }

            _context.Troops.RemoveRange(troops);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
