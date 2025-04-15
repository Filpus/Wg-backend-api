using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Actions")]
    [ApiController]
    public class ActionController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;

        public ActionController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;
            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Models.Action>>> GetActions(int? id)
        {
            if (id.HasValue)
            {
                var action = await _context.Actions.FindAsync(id);
                if (action == null)
                {
                    return NotFound();
                }
                return Ok(new List<Models.Action> { action });
            }
            else
            {
                return await _context.Actions.ToListAsync();
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutActions([FromBody] List<Models.Action> actions)
        {
            if (actions == null || actions.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var action in actions)
            {
                _context.Entry(action).State = EntityState.Modified;
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
        public async Task<ActionResult<Models.Action>> PostActions([FromBody] List<Models.Action> actions)
        {
            if (actions == null || actions.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (Models.Action action in actions)
            {
                action.Id = null;
            }

            _context.Actions.AddRange(actions);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetActions", new { id = actions[0].Id }, actions);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteActions([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var actions = await _context.Actions.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (actions.Count == 0)
            {
                return NotFound("Nie znaleziono akcji do usunięcia.");
            }

            _context.Actions.RemoveRange(actions);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
