using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers
{
    [Route("api/Religions")]
    [ApiController]
    public class ReligionsControler : Controller
    {
        private readonly AppDbContext _context;

        public ReligionsControler(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Religions
        // GET: api/Religions/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Religion>>> GetReligions(int? id)
        {
            if (id.HasValue)
            {
                var religion = await _context.Religions.FindAsync(id);
                if (religion == null)
                {
                    return NotFound();
                }
                return Ok(new List<Religion> { religion });
            }
            else
            {
                return Ok(await _context.Religions.ToListAsync());
            }
        }

        // PUT: api/Religions
        [HttpPut]
        public async Task<IActionResult> PutReligions([FromBody] List<Religion> religions)
        {
            if (religions == null || religions.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var religion in religions)
            {
                _context.Entry(religion).State = EntityState.Modified;
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

        // POST: api/Religions
        [HttpPost]
        public async Task<ActionResult<Religion>> PostReligions([FromBody] List<Religion> religions)
        {
            if (religions == null || religions.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }


            foreach (Religion religion in religions)
            {   
                if(religion.Name == null)
                {
                    return BadRequest("Brak nazwy religii.");
                }
                religion.Id = null;
            }

            _context.Religions.AddRange(religions);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReligions", new { id = religions[0].Id }, religions);
        }

        // DELETE: api/Religions
        [HttpDelete]
        public async Task<ActionResult> DeleteReligions([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var religions = await _context.Religions.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (religions.Count == 0)
            {
                return NotFound("Nie znaleziono religii do usunięcia.");
            }

            _context.Religions.RemoveRange(religions);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
