using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Events")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class EventsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private readonly ModifierProcessorFactory _processorFactory;
        private GameDbContext _context;
        private readonly int? _nationId;

        public EventsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, ModifierProcessorFactory processorFactory)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;
            this._processorFactory = processorFactory;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
                throw new InvalidOperationException("Brak schematu w sesji.");

            this._context = this._gameDbContextFactory.Create(schema);

            string nationIdStr = this._sessionDataService.GetNation();
            this._nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
        }

        [HttpPost]
        public async Task<ActionResult> CreateEvent([FromBody] EventDto dto)
        {
            var ev = new Event { Name = dto.Name, Description = dto.Description, IsActive = (bool)dto.IsActive };
            this._context.Add(ev);
            await this._context.SaveChangesAsync();

            foreach (var m in dto.Modifiers)
            {
                var mod = new Modifiers
                {
                    EventId = ev.Id.Value,
                    ModifierType = m.ModifierType,
                    
                    Effects = new ModifierEffect
                    {
                        Operation = m.Effect.Operation,
                        Value = (float)m.Effect.Value,
                        Conditions = m.Effect.Conditions
                    }
                };
                this._context.Add(mod);
            }

            await this._context.SaveChangesAsync();
            return CreatedAtAction(null, new { ev.Id });
        }

        [HttpDelete("{eventId}")]
        public async Task<ActionResult> DeleteEvent(int eventId)
        {
            var ev = await this._context.Events
                .Include(e => e.Modifiers)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
                return NotFound();

            var related = await this._context.RelatedEvents
                .Where(re => re.EventId == eventId)
                .ToListAsync();

            var modifiers = ev.Modifiers.ToList();

            foreach (var rel in related)
            {
                var nationId = rel.NationId;
                foreach (var group in modifiers.GroupBy(m => m.ModifierType))
                {
                    var processor = this._processorFactory.GetProcessor(group.Key);
                    var effects = group.Select(m => m.Effects).ToList();
                    await processor.RevertAsync(nationId, effects, this._context);
                }
            }

            this._context.RemoveRange(related);
            this._context.RemoveRange(ev.Modifiers);
            this._context.Remove(ev);
            await this._context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{eventId}")]
        public async Task<ActionResult> UpdateEvent(int eventId, [FromBody] EventDto dto)
        {
            var ev = await this._context.Events
                .Include(e => e.Modifiers)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
                return NotFound();

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.IsActive = (bool)dto.IsActive;
            this._context.Modifiers.RemoveRange(ev.Modifiers);

            foreach (var m in dto.Modifiers)
            {
                this._context.Add(new Modifiers
                {
                    EventId = eventId,
                    ModifierType = m.ModifierType,
                    Effects = new ModifierEffect
                    {
                        Operation = m.Effect.Operation,
                        Value = (float)m.Effect.Value,
                        Conditions = m.Effect.Conditions
                    }
                    
                });
            }

            await this._context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("modifiers")]
        public async Task<ActionResult> DeleteModifiers([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Brak ID do usunięcia.");

            var mods = await this._context.Modifiers.Where(m => ids.Contains(m.Id.Value)).ToListAsync();
            if (!mods.Any())
                return NotFound();

            this._context.Modifiers.RemoveRange(mods);
            await this._context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("assign")]
        public async Task<ActionResult> AssignEvent([FromBody] AssignEventDto dto)
        {
            this._context.Add(new RelatedEvents { EventId = dto.EventId, NationId = (int)(dto.NationId == null ? this._nationId.Value : dto.NationId) });
            await this._context.SaveChangesAsync();

            var modifiers = await this._context.Modifiers
                .Where(m => m.EventId == dto.EventId)
                .ToListAsync();

            foreach (var group in modifiers.GroupBy(m => m.ModifierType))
            {
                var processor = this._processorFactory.GetProcessor(group.Key);
                var effects = group.Select(m => m.Effects).ToList();
                await processor.ProcessAsync((int)(dto.NationId == null ? this._nationId.Value : dto.NationId), effects, this._context);
            }

            return Ok();
        }

        [HttpDelete("assign")]
        public async Task<ActionResult> UnassignEvent([FromBody] AssignEventDto dto)
        {


            var rel = await this._context.RelatedEvents
                .FirstOrDefaultAsync(r => r.EventId == dto.EventId && r.NationId == (dto.NationId ?? this._nationId));

            if (rel == null)
                return NotFound();

            this._context.Remove(rel);
            await this._context.SaveChangesAsync();

            var modifiers = await this._context.Modifiers
                .Where(m => m.EventId == dto.EventId)
                .ToListAsync();

            foreach (var group in modifiers.GroupBy(m => m.ModifierType))
            {
                var processor = this._processorFactory.GetProcessor(group.Key);
                var effects = group.Select(m => m.Effects).ToList();
                await processor.RevertAsync((int)(dto.NationId == null ? this._nationId.Value : dto.NationId), effects, this._context);
            }

            return Ok();
        }

        [HttpGet("assigned/{nationId?}")]
        public async Task<ActionResult<List<AssignEventInfoDto>>> GetAssignedEvents(int? nationId)
        {
            nationId ??= this._nationId;

            var assignedEvents = await this._context.RelatedEvents
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

        [HttpGet("{nationsId?}")]
        public async Task<ActionResult<List<EventDto>>> GetEvents(int? nationId)
        {
            if (!nationId.HasValue)
                nationId = _nationId;

            if (!nationId.HasValue)
                return BadRequest("Nation ID is required");

            var events = await _context.Events
                .Include(e => e.Modifiers)
                .Include(e => e.RelatedEvents)
                .Where(e => e.RelatedEvents.Any(re => re.NationId == nationId.Value))
                .ToListAsync();

            var eventDtos = events.Select(e => new EventDto
            {
                EventId = e.Id,
                Name = e.Name,
                Description = e.Description,
                ImageUrl = e.Picture,
                IsActive = e.IsActive,
                Modifiers = e.Modifiers.Any()
                    ? e.Modifiers.Select(m => new ModifierDto
                    {
                        ModifierId = m.Id,
                        ModifierType = m.ModifierType,
                        Effect = new ModifierEffectDto
                        {
                            Operation = m.Effects.Operation,
                            Value = (decimal)m.Effects.Value,
                            Conditions = m.Effects.Conditions
                        },
                        EffectCount = 1
                    }).ToList()
                    : new List<ModifierDto>()
            }).ToList();

            return Ok(eventDtos);
        }

        [HttpGet("allevents")]
        public async Task<ActionResult<List<EventDto>>> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Modifiers)
                .Include(e => e.RelatedEvents)
                .ToListAsync();

            var eventDtos = events.Select(e => new EventDto
            {
                EventId = e.Id,
                Name = e.Name,
                Description = e.Description,
                ImageUrl = e.Picture,
                IsActive = e.IsActive,
                Modifiers = e.Modifiers.Any()
                    ? e.Modifiers.Select(m => new ModifierDto
                    {
                        ModifierId = m.Id,
                        ModifierType = m.ModifierType,
                        Effect = m.Effects != null ? new ModifierEffectDto
                        {
                            Operation = m.Effects.Operation,
                            Value = (decimal)m.Effects.Value,
                            Conditions = m.Effects.Conditions
                        } : null,
                        EffectCount = 1
                    }).ToList()
                    : new List<ModifierDto>()
            }).ToList();

            return Ok(eventDtos);
        }

        [HttpGet("unassigned-nations/{eventId}")]
        public async Task<ActionResult<List<NationBaseInfoDTO>>> GetUnassignedNations(int eventId)
        {
            var assignedNationIds = await _context.RelatedEvents
                .Where(re => re.EventId == eventId)
                .Select(re => re.NationId)
                .ToListAsync();

            var unassignedNations = await _context.Nations
                .Where(n => !assignedNationIds.Contains(n.Id.Value))
                .Select(n => new NationBaseInfoDTO
                {
                    Id = n.Id,
                    Name = n.Name
                })
                .ToListAsync();

            return Ok(unassignedNations);
        }

        [HttpGet("assigned-nations/{eventId}")]
        public async Task<ActionResult<List<NationBaseInfoDTO>>> GetAssignedNations(int eventId)
        {
            var assignedNations = await _context.RelatedEvents
                .Where(re => re.EventId == eventId)
                .Include(re => re.Nation)
                .Select(re => new NationBaseInfoDTO
                {
                    Id = re.Nation.Id,
                    Name = re.Nation.Name
                })
                .ToListAsync();

            return Ok(assignedNations);
        }


        [HttpGet("option-pack")]
        public async Task<ActionResult<OptionPackDTO>> GetOptionPack()
        {
            var resources = await _context.Resources
                .Select(r => new ResourceDto { Id = (int)r.Id, Name = r.Name })
                .ToListAsync();

            var religions = await _context.Religions
                .Select(r => new ReligionDTO { Id = r.Id, Name = r.Name })
                .ToListAsync();

            var cultures = await _context.Cultures
                .Select(c => new CultureDTO { Id = c.Id, Name = c.Name })
                .ToListAsync();

            var socialGroups = await _context.SocialGroups
                .Select(sg => new SocialGroupInfoDTO
                {
                    Id = sg.Id,
                    Name = sg.Name,
                    BaseHappiness = sg.BaseHappiness,
                    Volunteers = sg.Volunteers,
                    ConsumedResources = new List<ResourceAmountDto>(),
                    ProducedResources = new List<ResourceAmountDto>()
                })
                .ToListAsync();

            var factions = await _context.Factions
                .Select(f => new FactionDTO { Id = f.Id, Name = f.Name })
                .ToListAsync();

            return Ok(new OptionPackDTO
            {
                Resources = resources,
                Religions = religions,
                Cultures = cultures,
                SocialGroups = socialGroups,
                Factions = factions
            });
        }
        [HttpGet("unassigned-events/{nationId?}")]
        public async Task<ActionResult<List<EventDto>>> GetUnassignedEvents(int? nationId)
        {
            if (!nationId.HasValue)
                nationId = _nationId;

            var assignedEventIds = await _context.RelatedEvents
                .Where(re => re.NationId == nationId)
                .Select(re => re.EventId)
                .ToListAsync();

            var unassignedEvents = await _context.Events
                .Where(e => !assignedEventIds.Contains(e.Id.Value))
                .Include(e => e.Modifiers)
                .Select(e => new EventDto
                {
                    EventId = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    ImageUrl = e.Picture,
                    IsActive = e.IsActive,
                    Modifiers =  new List<ModifierDto>()
                })
                .ToListAsync();

            return Ok(unassignedEvents);
        }
    }
}
