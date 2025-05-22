using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Events")]
    [ApiController]
    public class EventsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public EventsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;

            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }
            _context = _gameDbContextFactory.Create(schema);
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents(int? id)
        {
            if (id.HasValue)
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    return NotFound();
                }
                return Ok(new List<Event> { eventItem });
            }
            else
            {
                return await _context.Events.ToListAsync();
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutEvents([FromBody] List<Event> events)
        {
            if (events == null || events.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var eventItem in events)
            {
                _context.Entry(eventItem).State = EntityState.Modified;
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

        [HttpPost]
        public async Task<ActionResult<Event>> PostEvents([FromBody] List<Event> events)
        {
            if (events == null || events.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (Event eventItem in events)
            {
                eventItem.Id = null;
            }

            _context.Events.AddRange(events);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEvents", new { id = events[0].Id }, events);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteEvents([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var events = await _context.Events.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (events.Count == 0)
            {
                return NotFound("Nie znaleziono wydarzeń do usunięcia.");
            }

            _context.Events.RemoveRange(events);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
