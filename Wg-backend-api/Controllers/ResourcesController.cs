using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Wg_backend_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResourcesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Resources
        // GET: api/Resources/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResources(int? id)
        {
            if (id.HasValue)
            {
                var resource = await _context.Resources.FindAsync(id);
                if (resource == null)
                {
                    return NotFound();
                }
                return Ok(new List<Resource> { resource });  // Zwraca pojedynczy zasób w liście
            }
            else
            {
                return await _context.Resources.ToListAsync();  // Zwraca wszystkie zasoby
            }
        }

        // PUT: api/Resources
        [HttpPut]
        public async Task<IActionResult> PutResources([FromBody] List<Resource> resources)
        {
            if (resources == null || resources.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var resource in resources)
            {
                _context.Entry(resource).State = EntityState.Modified;
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

        // POST: api/Resources
        [HttpPost]
        public async Task<ActionResult<Resource>> PostResources([FromBody] List<Resource> resources)
        {
            if (resources == null || resources.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }


            
            foreach (Resource resource in resources)
            {
                resource.Id = null;
            }
            _context.Resources.AddRange(resources);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResources", new { id = resources[0].Id }, resources);
        }

        // DELETE: api/Resources
        [HttpDelete]
        public async Task<ActionResult> DeleteResources([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var resources = await _context.Resources.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (resources.Count == 0)
            {
                return NotFound("Nie znaleziono zasobów do usunięcia.");
            }

            _context.Resources.RemoveRange(resources);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
