using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/RelatedEvents")]
    [ApiController]
    public class RelatedEventsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;
        public RelatedEventsController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;

            string schema = HttpContext.Session.GetString("Schema");
            if (string.IsNullOrEmpty(schema))
            {
                throw new ArgumentNullException(nameof(schema), "Schema cannot be null or empty.");
            }
            _context = _gameDbContextFactory.Create(schema);
        }

        // GET: api/RelatedEvents
        // GET: api/RelatedEvents/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<RelatedEvents>>> GetRelatedEvents(int? id)
        {
            if (id.HasValue)
            {
                var relatedEvent = await _context.RelatedEvents.FindAsync(id);
                if (relatedEvent == null)
                {
                    return NotFound();
                }
                return Ok(new List<RelatedEvents> { relatedEvent });
            }
            else
            {
                return await _context.RelatedEvents.ToListAsync();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RelatedEvents>>> GetRelatedEvents()
        {
            return await _context.RelatedEvents.ToListAsync();
        }

        // PUT: api/RelatedEvents
        [HttpPut]
        public async Task<IActionResult> PutRelatedEvents([FromBody] List<RelatedEvents> relatedEvents)
        {
            if (relatedEvents == null || relatedEvents.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var relatedEvent in relatedEvents)
            {
                _context.Entry(relatedEvent).State = EntityState.Modified;
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

        // POST: api/RelatedEvents
        [HttpPost]
        public async Task<ActionResult<RelatedEvents>> PostRelatedEvents([FromBody] List<RelatedEvents> relatedEvents)
        {
            if (relatedEvents == null || relatedEvents.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (RelatedEvents relatedEvent in relatedEvents)
            {
                // Assuming RelatedEvents has an Id property
                relatedEvent.Id = null;
            }

            _context.RelatedEvents.AddRange(relatedEvents);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRelatedEvents", new { id = relatedEvents[0].Id }, relatedEvents);
        }

        // DELETE: api/RelatedEvents
        [HttpDelete]
        public async Task<ActionResult> DeleteRelatedEvents([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var relatedEvents = await _context.RelatedEvents.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (relatedEvents.Count == 0)
            {
                return NotFound("Nie znaleziono wydarzeń do usunięcia.");
            }

            _context.RelatedEvents.RemoveRange(relatedEvents);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
