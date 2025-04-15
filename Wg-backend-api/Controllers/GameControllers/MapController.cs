using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Maps")]
    [ApiController]
    public class MapController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;

        public MapController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;
            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Map>>> GetMaps(int? id)
        {
            if (id.HasValue)
            {
                var map = await _context.Maps.FindAsync(id);
                if (map == null)
                {
                    return NotFound();
                }
                return Ok(new List<Map> { map });
            }
            else
            {
                return await _context.Maps.ToListAsync();
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutMaps([FromBody] List<Map> maps)
        {
            if (maps == null || maps.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var map in maps)
            {
                _context.Entry(map).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "B³¹d podczas aktualizacji.");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Map>> PostMaps([FromBody] List<Map> maps)
        {
            if (maps == null || maps.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (Map map in maps)
            {
                map.Id = null;
            }

            _context.Maps.AddRange(maps);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMaps", new { id = maps[0].Id }, maps);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteMaps([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usuniêcia.");
            }

            var maps = await _context.Maps.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (maps.Count == 0)
            {
                return NotFound("Nie znaleziono map do usuniêcia.");
            }

            _context.Maps.RemoveRange(maps);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
