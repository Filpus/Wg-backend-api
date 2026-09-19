using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Logic.Modifiers;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Events")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class EventsController : GameControllerBase
    {
        private readonly ModifierProcessorFactory _processorFactory;

        public EventsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, ModifierProcessorFactory processorFactory)
            : base(gameDbFactory, sessionDataService)
        {
            this._processorFactory = processorFactory;
        }

        [HttpPost]
        public async Task<ActionResult> CreateEvent([FromBody] EventDto dto)
        {
            var ev = new Event { Name = dto.Name, Description = dto.Description, IsActive = (bool)dto.IsActive };
            this.Context.Add(ev);
            await this.Context.SaveChangesAsync();

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
                this.Context.Add(mod);
            }

            await this.Context.SaveChangesAsync();
            return CreatedAtAction(null, new { ev.Id });
        }

        [HttpDelete("{eventId}")]
        public async Task<ActionResult> DeleteEvent(int eventId)
        {
            var ev = await this.Context.Events
                .Include(e => e.Modifiers)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
            {
                return NotFound();
            }

            var related = await this.Context.RelatedEvents
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
                    await processor.RevertAsync(nationId, effects, this.Context);
                }
            }

            this.Context.RemoveRange(related);
            this.Context.RemoveRange(ev.Modifiers);
            this.Context.Remove(ev);
            await this.Context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{eventId}")]
        public async Task<ActionResult> UpdateEvent(int eventId, [FromBody] EventDto dto)
        {
            var ev = await this.Context.Events
                .Include(e => e.Modifiers)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
            {
                return NotFound();
            }

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.IsActive = (bool)dto.IsActive;
            this.Context.Modifiers.RemoveRange(ev.Modifiers);

            foreach (var m in dto.Modifiers)
            {
                this.Context.Add(new Modifiers
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

            await this.Context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("modifiers")]
        public async Task<ActionResult> DeleteModifiers([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var mods = await this.Context.Modifiers.Where(m => ids.Contains(m.Id.Value)).ToListAsync();
            if (!mods.Any())
            {
                return NotFound();
            }

            this.Context.Modifiers.RemoveRange(mods);
            await this.Context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("assign")]
        public async Task<ActionResult> AssignEvent([FromBody] AssignEventDto dto)
        {
            this.Context.Add(new RelatedEvents { EventId = dto.EventId, NationId = (int)(dto.NationId == null ? this.NationId.Value : dto.NationId) });
            await this.Context.SaveChangesAsync();

            var modifiers = await this.Context.Modifiers
                .Where(m => m.EventId == dto.EventId)
                .ToListAsync();

            foreach (var group in modifiers.GroupBy(m => m.ModifierType))
            {
                var processor = this._processorFactory.GetProcessor(group.Key);
                var effects = group.Select(m => m.Effects).ToList();
                await processor.ProcessAsync((int)(dto.NationId == null ? this.NationId.Value : dto.NationId), effects, this.Context);
            }

            return Ok();
        }

        [HttpDelete("assign")]
        public async Task<ActionResult> UnassignEvent([FromBody] AssignEventDto dto)
        {

            var rel = await this.Context.RelatedEvents
                .FirstOrDefaultAsync(r => r.EventId == dto.EventId && r.NationId == (dto.NationId ?? this.NationId));

            if (rel == null)
            {
                return NotFound();
            }

            this.Context.Remove(rel);
            await this.Context.SaveChangesAsync();

            var modifiers = await this.Context.Modifiers
                .Where(m => m.EventId == dto.EventId)
                .ToListAsync();

            foreach (var group in modifiers.GroupBy(m => m.ModifierType))
            {
                var processor = this._processorFactory.GetProcessor(group.Key);
                var effects = group.Select(m => m.Effects).ToList();
                await processor.RevertAsync((int)(dto.NationId == null ? this.NationId.Value : dto.NationId), effects, this.Context);
            }

            return Ok();
        }

        [HttpGet("assigned/{nationId?}")]
        public async Task<ActionResult<List<AssignEventInfoDto>>> GetAssignedEvents(int? nationId)
        {
            nationId ??= this.NationId;

            var assignedEvents = await this.Context.RelatedEvents
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
            {
                nationId = this.NationId;
            }

            if (!nationId.HasValue)
            {
                return BadRequest("Nation ID is required");
            }

            var events = await this.Context.Events
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
                    ? [.. e.Modifiers.Select(m => new ModifierDto
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
                    })]
                    : []
            }).ToList();

            return Ok(eventDtos);
        }

        [HttpGet("allevents")]
        public async Task<ActionResult<List<EventDto>>> GetAllEvents()
        {
            var events = await this.Context.Events
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
                    ? [.. e.Modifiers.Select(m => new ModifierDto
                    {
                        ModifierId = m.Id,
                        ModifierType = m.ModifierType,
                        Effect = m.Effects != null ? new ModifierEffectDto
                        {
                            Operation = m.Effects.Operation,
                            Value = (decimal)m.Effects.Value,
                            Conditions = m.Effects.Conditions,
                        }
                        : null,
                        EffectCount = 1,
                    })]
                    : [],
            }).ToList();

            return Ok(eventDtos);
        }

        [HttpGet("unassigned-nations/{eventId}")]
        public async Task<ActionResult<List<NationBaseInfoDTO>>> GetUnassignedNations(int eventId)
        {
            var assignedNationIds = await this.Context.RelatedEvents
                .Where(re => re.EventId == eventId)
                .Select(re => re.NationId)
                .ToListAsync();

            var unassignedNations = await this.Context.Nations
                .Where(n => !assignedNationIds.Contains(n.Id.Value))
                .Select(n => new NationBaseInfoDTO
                {
                    Id = n.Id,
                    Name = n.Name,
                })
                .ToListAsync();

            return Ok(unassignedNations);
        }

        [HttpGet("assigned-nations/{eventId}")]
        public async Task<ActionResult<List<NationBaseInfoDTO>>> GetAssignedNations(int eventId)
        {
            var assignedNations = await this.Context.RelatedEvents
                .Where(re => re.EventId == eventId)
                .Include(re => re.Nation)
                .Select(re => new NationBaseInfoDTO
                {
                    Id = re.Nation.Id,
                    Name = re.Nation.Name,
                })
                .ToListAsync();

            return Ok(assignedNations);
        }

        [HttpGet("option-pack")]
        public async Task<ActionResult<OptionPackDTO>> GetOptionPack()
        {
            var resources = await this.Context.Resources
                .Select(r => new ResourceDto { Id = (int)r.Id, Name = r.Name })
                .ToListAsync();

            var religions = await this.Context.Religions
                .Select(r => new ReligionDTO { Id = r.Id, Name = r.Name })
                .ToListAsync();

            var cultures = await this.Context.Cultures
                .Select(c => new CultureDTO { Id = c.Id, Name = c.Name })
                .ToListAsync();

            var socialGroups = await this.Context.SocialGroups
                .Select(sg => new SocialGroupInfoDTO
                {
                    Id = sg.Id,
                    Name = sg.Name,
                    BaseHappiness = sg.BaseHappiness,
                    Volunteers = sg.Volunteers,
                    ConsumedResources = new List<ResourceAmountDto>(),
                    ProducedResources = new List<ResourceAmountDto>(),
                })
                .ToListAsync();

            var factions = await this.Context.Factions
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
            {
                nationId = this.NationId;
            }

            var assignedEventIds = await this.Context.RelatedEvents
                .Where(re => re.NationId == nationId)
                .Select(re => re.EventId)
                .ToListAsync();

            var unassignedEvents = await this.Context.Events
                .Where(e => !assignedEventIds.Contains(e.Id.Value))
                .Include(e => e.Modifiers)
                .Select(e => new EventDto
                {
                    EventId = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    ImageUrl = e.Picture,
                    IsActive = e.IsActive,
                    Modifiers = new List<ModifierDto>()
                })
                .ToListAsync();

            return Ok(unassignedEvents);
        }
    }
}
