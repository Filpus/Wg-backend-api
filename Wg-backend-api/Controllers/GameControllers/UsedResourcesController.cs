using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/UsedResources")]
    [ApiController]
    public class UsedResourcesController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;

        public UsedResourcesController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;

            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        // GET: api/UsedResources
        // GET: api/UsedResources/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<UsedResource>>> GetUsedResources(int? id)
        {
            if (id.HasValue)
            {
                var usedResource = await _context.UsedResources.FindAsync(id);
                if (usedResource == null)
                {
                    return NotFound();
                }
                return Ok(new List<UsedResource> { usedResource });
            }
            else
            {
                return await _context.UsedResources.ToListAsync();
            }
        }

        // PUT: api/UsedResources
        [HttpPut]
        public async Task<IActionResult> PutUsedResources([FromBody] List<UsedResource> usedResources)
        {
            if (usedResources == null || usedResources.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var usedResource in usedResources)
            {
                _context.Entry(usedResource).State = EntityState.Modified;
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

        // POST: api/UsedResources
        [HttpPost]
        public async Task<ActionResult<UsedResource>> PostUsedResources([FromBody] List<UsedResource> usedResources)
        {
            if (usedResources == null || usedResources.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (UsedResource usedResource in usedResources)
            {
                usedResource.Id = null;
            }

            _context.UsedResources.AddRange(usedResources);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsedResources", new { id = usedResources[0].Id }, usedResources);
        }

        // DELETE: api/UsedResources
        [HttpDelete]
        public async Task<ActionResult> DeleteUsedResources([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var usedResources = await _context.UsedResources.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (usedResources.Count == 0)
            {
                return NotFound("Nie znaleziono zasobów do usunięcia.");
            }

            _context.UsedResources.RemoveRange(usedResources);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
