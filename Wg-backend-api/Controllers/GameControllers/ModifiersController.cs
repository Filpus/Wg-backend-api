using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;


namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Modifiers")]
    [ApiController]
    public class ModifiersController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;
        public ModifiersController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;

            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        // GET: api/Modifiers
        // GET: api/Modifiers/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Modifiers>>> GetModifiers(int? id)
        {
            if (id.HasValue)
            {
                var modifier = await _context.Modifiers.FindAsync(id);
                if (modifier == null)
                {
                    return NotFound();
                }
                return Ok(new List<Modifiers> { modifier });
            }
            else
            {
                return await _context.Modifiers.ToListAsync();
            }
        }

        // PUT: api/Modifiers
        [HttpPut]
        public async Task<IActionResult> PutModifiers([FromBody] List<Modifiers> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var modifier in modifiers)
            {
                _context.Entry(modifier).State = EntityState.Modified;
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

        // POST: api/Modifiers
        [HttpPost]
        public async Task<ActionResult<Modifiers>> PostModifiers([FromBody] List<Modifiers> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (Modifiers modifier in modifiers)
            {
                modifier.Id = null;
            }

            _context.Modifiers.AddRange(modifiers);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetModifiers", new { id = modifiers[0].Id }, modifiers);
        }

        // DELETE: api/Modifiers
        [HttpDelete]
        public async Task<ActionResult> DeleteModifiers([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var modifiers = await _context.Modifiers.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (modifiers.Count == 0)
            {
                return NotFound("Nie znaleziono modyfikatorów do usunięcia.");
            }

            _context.Modifiers.RemoveRange(modifiers);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
