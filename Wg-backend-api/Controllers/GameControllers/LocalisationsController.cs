using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Localisations")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class LocalisationsController : ControllerBase
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int? _nationId;

        public LocalisationsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = this._gameDbContextFactory.Create(schema);
            string nationIdStr = this._sessionDataService.GetNation();
            this._nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
        }
        // GET: api/Localisations  
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocalisationDTO>>> GetLocalisation()
        {
            return await this._context.Localisations
                .Select(l => new LocalisationDTO
                {
                    Id = l.Id,
                    Name = l.Name,
                    NationId = l.NationId,
                    Fortification = l.Fortification,
                    Size = l.Size

                })
                .ToListAsync();
        }

        // GET: api/Localisations/5  
        [HttpGet("{id}")]
        public async Task<ActionResult<LocalisationDTO>> GetLocalisation(int? id)
        {
            var localisation = await this._context.Localisations
                .Where(l => l.Id == id)
                .Select(l => new LocalisationDTO
                {
                    Id = l.Id,
                    Name = l.Name,
                    NationId = l.NationId,
                    Fortification = l.Fortification,
                    Size = l.Size

                })
                .FirstOrDefaultAsync();

            if (localisation == null)
            {
                return NotFound();
            }

            return localisation;
        }

        [HttpPut]
        public async Task<IActionResult> PutLocalisation(List<LocalisationDTO> localisationDtos)
        {
            foreach (var localisationDto in localisationDtos)
            {
                var localisation = await this._context.Localisations.FindAsync(localisationDto.Id);
                if (localisation == null)
                {
                    return NotFound($"Localisation with ID {localisationDto.Id} not found.");
                }

                localisation.Name = localisationDto.Name;
                localisation.NationId = localisationDto.NationId;
                localisation.Size = localisationDto.Size;
                localisation.Fortification = localisationDto.Fortification;

                this._context.Entry(localisation).State = EntityState.Modified;
            }

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var localisationDto in localisationDtos)
                {
                    if (!LocalisationExists(localisationDto.Id))
                    {
                        return NotFound($"Localisation with ID {localisationDto.Id} not found.");
                    }
                }

                throw;
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<LocalisationDTO>>> PostLocalisations(List<LocalisationDTO> localisationDtos)
        {
            var localisations = new List<Localisation>();

            foreach (var localisationDto in localisationDtos)
            {
                var localisation = new Localisation
                {
                    Name = localisationDto.Name,
                    NationId = localisationDto.NationId,
                    Fortification = localisationDto.Fortification,
                    Size = localisationDto.Size
                };

                localisations.Add(localisation);
            }

            this._context.Localisations.AddRange(localisations);
            await this._context.SaveChangesAsync();

            for (int i = 0; i < localisations.Count; i++)
            {
                localisationDtos[i].Id = localisations[i].Id;
            }

            return CreatedAtAction("GetLocalisation", localisationDtos);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLocalisations([FromBody] List<int?> ids)
        {
            var localisations = await this._context.Localisations
                .Where(l => ids.Contains(l.Id))
                .ToListAsync();

            if (localisations == null || !localisations.Any())
            {
                return NotFound("No localisations found for the provided IDs.");
            }

            this._context.Localisations.RemoveRange(localisations);
            await this._context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocalisationExists(int? id)
        {
            return this._context.Localisations.Any(e => e.Id == id);
        }

        [HttpGet("Nation/GeneralInfo/{nationId?}")]
        public async Task<ActionResult<IEnumerable<LocalisationGeneralInfoDTO>>> GetLocalisationsGeneralInfoByNation(int? nationId)
        {
            nationId ??= this._nationId ?? throw new InvalidOperationException("Brak ID narodu w sesji.");
            var localisations = await this._context.Localisations
                .Where(l => l.NationId == nationId)
                .Select(l => new LocalisationGeneralInfoDTO
                {
                    Id = (int)l.Id,
                    Name = l.Name,
                    Size = l.Size,
                    Fortification = l.Fortification,
                    PopulationSize = this._context.Populations.Where(p => p.Location.Id == l.Id).Count(),
                    PopulationHappiness = this._context.Populations.Where(p => p.Location.Id == l.Id).Average(p => p.Happiness)
                })
                .ToListAsync();

            return Ok(localisations);
        }

        [HttpGet("Details/{id}")]
        public async Task<ActionResult<LocalisationDetailsDTO>> GetLocalisationDetails(int id)
        {
            var localisation = await this._context.Localisations
                .Where(l => l.Id == id)
                .Select(l => new LocalisationDetailsDTO
                {
                    Name = l.Name,
                    Resources = GetLocalisationResources(id),
                    ResourceProductions = GetLocalisationResourceProductions(id),
                    PopulationGroups = GetPopulationGroups(id)
                })
                .FirstOrDefaultAsync();

            if (localisation == null)
            {
                return NotFound();
            }

            return Ok(localisation);
        }

        private List<LocalisationResourceProductionDTO> GetLocalisationResourceProductions(int localisationId)
        {
            var localisationResources = this._context.LocalisationResources
                   .Include(lr => lr.Resource)
                   .Where(lr => lr.LocationId == localisationId)
                   .ToList();

            // Pobierz populacje i podziel na grupy społeczne  
            var populationGroups = this._context.Populations
                .Where(p => p.LocationId == localisationId)
                .GroupBy(p => p.SocialGroupId)
                .Select(g => new
                {
                    SocialGroupId = g.Key,
                    PopulationCount = g.Count()
                })
                .ToList();

            // Pobierz wszystkie udziały produkcyjne dla tej lokalizacji  
            var productionShares = this._context.ProductionShares.ToList();

            var result = new List<LocalisationResourceProductionDTO>();

            foreach (var lr in localisationResources)
            {
                float totalProduction = 0;

                foreach (var group in populationGroups)
                {
                    var share = productionShares
                        .FirstOrDefault(ps => ps.SocialGroupId == group.SocialGroupId && ps.ResourceId == lr.ResourceId);

                    if (share != null)
                    {
                        totalProduction += group.PopulationCount * share.Coefficient * lr.Amount;
                    }
                }

                result.Add(new LocalisationResourceProductionDTO
                {
                    ResourceName = lr.Resource.Name,
                    ResourceId = lr.Resource.Id.Value,
                    ProductionAmount = totalProduction,
                });
            }

            return result;
        }

        private List<PopulationGroupDTO> GetPopulationGroups(int localisationId)
        {
            var populationGroups = this._context.Populations
                .Where(p => p.LocationId == localisationId)
                .GroupBy(p => new { p.ReligionId, p.CultureId, p.SocialGroupId })
                .Select(g => new PopulationGroupDTO
                {
                    ReligionId = g.Key.ReligionId,
                    CultureId = g.Key.CultureId,
                    SocialGroupId = g.Key.SocialGroupId,

                    Religion = this._context.Religions.Where(r => r.Id == g.Key.ReligionId).Select(r => r.Name).FirstOrDefault() ?? string.Empty,
                    Culture = this._context.Cultures.Where(r => r.Id == g.Key.CultureId).Select(r => r.Name).FirstOrDefault() ?? string.Empty,
                    SocialGroup = this._context.SocialGroups.Where(s => s.Id == g.Key.SocialGroupId).Select(r => r.Name).FirstOrDefault() ?? string.Empty,

                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness),

                })
                .ToList();

            return populationGroups;
        }

        private List<LocalisationResourceInfoDTO> GetLocalisationResources(int localisationId)
        {
            return [.. this._context.LocalisationResources
                .Where(lr => lr.LocationId == localisationId)
                .Select(lr => new LocalisationResourceInfoDTO
                {
                    ResourceId = lr.Resource.Id.Value,
                    ResourceName = lr.Resource.Name,
                    Amount = lr.Amount,
                })];
        }

        // POST: api/Localisations/Resources
        [HttpPost("Resources")]
        public async Task<IActionResult> CreateLocalisationResources([FromBody] List<LocalisationResourceDTO> localisationResourceDtos)
        {
            if (localisationResourceDtos == null || !localisationResourceDtos.Any())
            {
                return BadRequest("Invalid data.");
            }

            var localisationResources = localisationResourceDtos.Select(dto => new LocalisationResource
            {
                LocationId = dto.LocationId,
                ResourceId = dto.ResourceId,
                Amount = dto.Amount,
            }).ToList();

            this._context.LocalisationResources.AddRange(localisationResources);
            await this._context.SaveChangesAsync();

            for (int i = 0; i < localisationResources.Count; i++)
            {
                localisationResourceDtos[i].Id = localisationResources[i].Id;
            }

            return Ok(localisationResourceDtos);
        }

        // PUT: api/Localisations/Resources
        [HttpPut("Resources")]
        public async Task<IActionResult> UpdateLocalisationResources([FromBody] List<LocalisationResourceDTO> localisationResourceDtos)
        {
            if (localisationResourceDtos == null || !localisationResourceDtos.Any())
            {
                return BadRequest("Invalid data.");
            }

            foreach (var dto in localisationResourceDtos)
            {
                var localisationResource = await this._context.LocalisationResources
                    .FirstOrDefaultAsync(lr => lr.LocationId == dto.LocationId && lr.ResourceId == dto.ResourceId);

                if (localisationResource == null)
                {
                    return NotFound($"LocalisationResource with LocationId {dto.LocationId} and ResourceId {dto.ResourceId} not found.");
                }

                localisationResource.LocationId = dto.LocationId;
                localisationResource.ResourceId = dto.ResourceId;
                localisationResource.Amount = dto.Amount;

                this._context.Entry(localisationResource).State = EntityState.Modified;
            }

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var dto in localisationResourceDtos)
                {
                    var exists = await this._context.LocalisationResources
                        .AnyAsync(lr => lr.LocationId == dto.LocationId && lr.ResourceId == dto.ResourceId);

                    if (!exists)
                    {
                        return NotFound($"LocalisationResource with LocationId {dto.LocationId} and ResourceId {dto.ResourceId} not found.");
                    }
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/Localisations/Resources
        [HttpDelete("Resources")]
        public async Task<IActionResult> DeleteLocalisationResources([FromBody] List<LocalisationResourceDTO> keys)
        {
            if (keys == null || !keys.Any())
            {
                return BadRequest("Invalid data.");
            }

            var locationIds = keys.Select(k => k.LocationId).Distinct().ToList();
            var resourceIds = keys.Select(k => k.ResourceId).Distinct().ToList();

            var candidates = await this._context.LocalisationResources
                .Where(lr => locationIds.Contains(lr.LocationId) && resourceIds.Contains(lr.ResourceId))
                .ToListAsync();

            var localisationResources = candidates
                .Where(lr => keys.Any(k => k.LocationId == lr.LocationId && k.ResourceId == lr.ResourceId))
                .ToList();

            if (localisationResources == null || !localisationResources.Any())
            {
                return NotFound("No LocalisationResources found for the provided LocationId/ResourceId pairs.");
            }

            this._context.LocalisationResources.RemoveRange(localisationResources);
            await this._context.SaveChangesAsync();

            return NoContent();
        }
    }
}
