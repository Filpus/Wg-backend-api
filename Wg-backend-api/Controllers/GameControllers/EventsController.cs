using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Logic.Modifiers;
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
        private readonly ModifierProcessorFactory _processorFactory;
        private GameDbContext _context;
        private readonly int? _nationId;

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

            string nationIdStr = _sessionDataService.GetNation();
            _nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
        }


        [HttpPost]
        public async Task<ActionResult> CreateEvent([FromBody] EventDto dto)
        {
            var ev = new Event { Name = dto.Name, Description = dto.Description };
            _context.Add(ev);
            await _context.SaveChangesAsync();
            foreach (var m in dto.Modifiers)
            {
                var mod = new Modifiers
                {
                    EventId = ev.Id.Value,
                    modiferType = m.ModifierType,
                    Effects = JsonSerializer.Serialize(new[]
                    {
                    new { Operation = m.Effect.Operation, Value = m.Effect.Value, Conditions = m.Effect.Conditions.ToDictionary() }
                })
                };
                _context.Add(mod);
            }
            await _context.SaveChangesAsync();
            return CreatedAtAction(null, new { ev.Id });
        }

        [HttpDelete("{eventId}")]
        public async Task<ActionResult> DeleteEvent(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Modifiers)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null)
                return NotFound();

            var related = await _context.RelatedEvents
                .Where(re => re.EventId == eventId)
                .ToListAsync();

            var modifiers = ev.Modifiers.ToList();
            foreach (var rel in related)
            {
                var nationId = rel.NationId;
                foreach (var group in modifiers.GroupBy(m => m.modiferType))
                {
                    var processor = _processorFactory.GetProcessor(group.Key);
                    var effects = group
                        .Select(m => JsonSerializer.Deserialize<ModifierEffect>(m.Effects)!)
                        .ToList();
                    await processor.RevertAsync(nationId, effects, _context);
                }
            }

            _context.RemoveRange(related);

            _context.RemoveRange(ev.Modifiers);
            _context.Remove(ev);

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Edit Event + Modifiers
        [HttpPut("{eventId}")]
        public async Task<ActionResult> UpdateEvent(int eventId, [FromBody] EventDto dto)
        {
            var ev = await _context.Events
                .Include(e => e.Modifiers)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();
            ev.Name = dto.Name;
            ev.Description = dto.Description;
            _context.Modifiers.RemoveRange(ev.Modifiers);
            foreach (var m in dto.Modifiers)
            {
                _context.Add(new Modifiers
                {
                    EventId = eventId,
                    modiferType = m.ModifierType,
                    Effects = JsonSerializer.Serialize(new[]
                    {
                    new { Operation = m.Effect.Operation, Value = m.Effect.Value, Conditions = m.Effect.Conditions.ToDictionary() }
                })
                });
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        // Delete only Modifiers by IDs
        [HttpDelete("modifiers")]
        public async Task<ActionResult> DeleteModifiers([FromBody] List<int?> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Brak ID do usunięcia.");
            var mods = await _context.Modifiers.Where(m => ids.Contains(m.Id)).ToListAsync();
            if (!mods.Any()) return NotFound();
            _context.Modifiers.RemoveRange(mods);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("assign")]
        public async Task<ActionResult> AssignEvent([FromBody] AssignEventDto dto)
        {
            _context.Add(new RelatedEvents { EventId = dto.EventId, NationId = dto.NationId });
            await _context.SaveChangesAsync();

            var modifiers = await _context.Modifiers
                .Where(m => m.EventId == dto.EventId)
                .ToListAsync();

            foreach (var group in modifiers.GroupBy(m => m.modiferType))
            {
                var processor = _processorFactory.GetProcessor(group.Key);
                var effects = group
                    .Select(m => JsonSerializer.Deserialize<ModifierEffect>(m.Effects)!)
                    .ToList();
                await processor.ProcessAsync(dto.NationId, effects, _context);
            }

            return Ok();
        }


        [HttpDelete("assign")]
        public async Task<ActionResult> UnassignEvent([FromBody] AssignEventDto dto)
        {

            var rel = await _context.RelatedEvents
                .FirstOrDefaultAsync(r => r.EventId == dto.EventId && r.NationId == dto.NationId);
            if (rel == null) return NotFound();
            _context.Remove(rel);
            await _context.SaveChangesAsync();


            var modifiers = await _context.Modifiers
                .Where(m => m.EventId == dto.EventId)
                .ToListAsync();


            foreach (var group in modifiers.GroupBy(m => m.modiferType))
            {
                var processor = _processorFactory.GetProcessor(group.Key);
                var effects = group
                    .Select(m => JsonSerializer.Deserialize<ModifierEffect>(m.Effects)!)
                    .ToList();
                await processor.RevertAsync(dto.NationId, effects, _context);
            }

            return Ok();
        }

        [HttpGet("assigned/{nationId?}")]
        public async Task<ActionResult<List<AssignEventInfoDto>>> GetAssignedEvents(int? nationId)
        {

            if (nationId == null)
            {
                nationId = _nationId;
            }
            var assignedEvents = await _context.RelatedEvents
                .Where(re => re.NationId == nationId)
                .Include(re => re.Event)
                .Include(re => re.Nation)
                .Select(re => new AssignEventInfoDto
                {
                    EventId = re.EventId,
                    EventName = re.Event.Name,
                    EventDescription = re.Event.Description,
                    NationId = (int)nationId,
                    NationName = re.Nation.Name
                })
                .ToListAsync();

            return Ok(assignedEvents);
        }

    }
}
