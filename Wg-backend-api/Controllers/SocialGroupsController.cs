using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocialGroupsController : Controller
    {
        private readonly AppDbContext _context;

        public SocialGroupsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/SocialGroups
        // GET: api/SocialGroups/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<SocialGroup>>> GetSocialGroups(int? id)
        {
            if (id.HasValue)
            {
                var socialGroup = await _context.SocialGroups.FindAsync(id);
                if (socialGroup == null)
                {
                    return NotFound();
                }
                return Ok(new List<SocialGroup> { socialGroup });
            }
            else
            {
                return await _context.SocialGroups.ToListAsync();
            }
        }

        // PUT: api/SocialGroups
        [HttpPut]
        public async Task<IActionResult> PutSocialGroups([FromBody] List<SocialGroup> socialGroups)
        {
            if (socialGroups == null || socialGroups.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var socialGroup in socialGroups)
            {
                _context.Entry(socialGroup).State = EntityState.Modified;
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

        // POST: api/SocialGroups
        [HttpPost]
        public async Task<ActionResult<SocialGroup>> PostSocialGroups([FromBody] List<SocialGroup> socialGroups)
        {
            if (socialGroups == null || socialGroups.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }
            foreach( SocialGroup group in socialGroups)
            {
                group.Id = null;
            }

            _context.SocialGroups.AddRange(socialGroups);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSocialGroups", new { id = socialGroups[0].Id }, socialGroups);
        }

        // DELETE: api/SocialGroups
        [HttpDelete]
        public async Task<ActionResult> DeleteSocialGroups([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var socialGroups = await _context.SocialGroups.Where(s => ids.Contains(s.Id)).ToListAsync();

            if (socialGroups.Count == 0)
            {
                return NotFound("Nie znaleziono grup społecznych do usunięcia.");
            }

            _context.SocialGroups.RemoveRange(socialGroups);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
