using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/UnitTypes")]
    [ApiController]
    public class UnitTypeController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;

        public UnitTypeController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;

            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        // GET: api/UnitTypes
        // GET: api/UnitTypes/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<UnitType>>> GetUnitTypes(int? id)
        {
            if (id.HasValue)
            {
                var unitType = await _context.UnitTypes.FindAsync(id);
                if (unitType == null)
                {
                    return NotFound();
                }
                return Ok(new List<UnitType> { unitType });
            }
            else
            {
                return await _context.UnitTypes.ToListAsync();
            }
        }

        // PUT: api/UnitTypes
        [HttpPut]
        public async Task<IActionResult> PutUnitTypes([FromBody] List<UnitType> unitTypes)
        {
            if (unitTypes == null || unitTypes.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var unitType in unitTypes)
            {
                _context.Entry(unitType).State = EntityState.Modified;
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

        // POST: api/UnitTypes
        [HttpPost]
        public async Task<ActionResult<UnitType>> PostUnitTypes([FromBody] List<UnitType> unitTypes)
        {
            if (unitTypes == null || unitTypes.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (UnitType unitType in unitTypes)
            {
                unitType.Id = null;
            }

            _context.UnitTypes.AddRange(unitTypes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnitTypes", new { id = unitTypes[0].Id }, unitTypes);
        }

        // DELETE: api/UnitTypes
        [HttpDelete]
        public async Task<ActionResult> DeleteUnitTypes([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var unitTypes = await _context.UnitTypes.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (unitTypes.Count == 0)
            {
                return NotFound("Nie znaleziono jednostek do usunięcia.");
            }

            _context.UnitTypes.RemoveRange(unitTypes);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
