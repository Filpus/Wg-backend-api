using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers
{
    [Route("api/Localisations")]
    [ApiController]
    public class LocalisationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocalisationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Localisations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Localisation>>> GetLocalisation()
        {
            return await _context.Localisation.ToListAsync();
        }

        // GET: api/Localisations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Localisation>> GetLocalisation(int? id)
        {
            var localisation = await _context.Localisation.FindAsync(id);

            if (localisation == null)
            {
                return NotFound();
            }

            return localisation;
        }

        // PUT: api/Localisations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocalisation(int? id, Localisation localisation)
        {
            if (id != localisation.Id)
            {
                return BadRequest();
            }

            _context.Entry(localisation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocalisationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Localisations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Localisation>> PostLocalisation(Localisation localisation)
        {

            localisation.Id = null;
            _context.Localisation.Add(localisation);
            await _context.SaveChangesAsync();


            return CreatedAtAction("GetLocalisation", new { id = localisation.Id }, localisation);
        }

        // DELETE: api/Localisations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocalisation(int? id)
        {
            var localisation = await _context.Localisation.FindAsync(id);
            if (localisation == null)
            {
                return NotFound();
            }

            _context.Localisation.Remove(localisation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocalisationExists(int? id)
        {
            return _context.Localisation.Any(e => e.Id == id);
        }
    }
}
