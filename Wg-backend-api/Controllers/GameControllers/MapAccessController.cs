using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/MapAccesses")]
    [ApiController]
    public class MapAccessController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;

        public MapAccessController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;
            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        [HttpGet("{userId}/{mapId}")]
        public async Task<ActionResult<MapAccess>> GetMapAccess(int userId, int mapId)
        {
            var mapAccess = await _context.MapAccesses.FindAsync(userId, mapId);
            if (mapAccess == null)
            {
                return NotFound();
            }
            return Ok(mapAccess);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MapAccess>>> GetMapAccesses()
        {
            return await _context.MapAccesses.ToListAsync();
        }

        [HttpPut]
        public async Task<IActionResult> PutMapAccesses([FromBody] List<MapAccess> mapAccesses)
        {
            if (mapAccesses == null || mapAccesses.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var mapAccess in mapAccesses)
            {
                _context.Entry(mapAccess).State = EntityState.Modified;
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
        public async Task<ActionResult<MapAccess>> PostMapAccesses([FromBody] List<MapAccess> mapAccesses)
        {
            if (mapAccesses == null || mapAccesses.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            _context.MapAccesses.AddRange(mapAccesses);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMapAccesses", new { userId = mapAccesses[0].UserId, mapId = mapAccesses[0].MapId }, mapAccesses);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteMapAccesses([FromBody] List<(int userId, int mapId)> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usuniêcia.");
            }

            var mapAccesses = await _context.MapAccesses
               .Where(ma => ids.Any(id => id.userId == ma.UserId && id.mapId == ma.MapId))
               .ToListAsync();

            if (mapAccesses.Count == 0)
            {
                return NotFound("Nie znaleziono dostêpu do map do usuniêcia.");
            }

            _context.MapAccesses.RemoveRange(mapAccesses);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
