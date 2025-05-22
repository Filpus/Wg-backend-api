using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Localisations")]
    [ApiController]
    public class LocalisationsController : ControllerBase
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public LocalisationsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        // GET: api/Localisations  
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocalisationDTO>>> GetLocalisation()
        {
            return await _context.Localisations
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
            var localisation = await _context.Localisations
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

        // PUT: api/Localisations/5  
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocalisation(int? id, LocalisationDTO localisationDto)
        {
            if (id != localisationDto.Id)
            {
                return BadRequest();
            }

            var localisation = await _context.Localisations.FindAsync(id);
            if (localisation == null)
            {
                return NotFound();
            }

            localisation.Name = localisationDto.Name;
            localisation.NationId = localisationDto.NationId;

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
        [HttpPost]
        public async Task<ActionResult<LocalisationDTO>> PostLocalisation(LocalisationDTO localisationDto)
        {
            var localisation = new Localisation
            {
                Name = localisationDto.Name,
                NationId = localisationDto.NationId,
                Fortification = localisationDto.Fortification,
                Size = localisationDto.Size
            };

            _context.Localisations.Add(localisation);
            await _context.SaveChangesAsync();

            localisationDto.Id = localisation.Id;

            return CreatedAtAction("GetLocalisation", new { id = localisation.Id }, localisationDto);
        }

        // DELETE: api/Localisations/5  
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocalisation(int? id)
        {
            var localisation = await _context.Localisations.FindAsync(id);
            if (localisation == null)
            {
                return NotFound();
            }

            _context.Localisations.Remove(localisation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocalisationExists(int? id)
        {
            return _context.Localisations.Any(e => e.Id == id);
        }

        [HttpGet("Nation/{nationId}/GeneralInfo")]
        public async Task<ActionResult<IEnumerable<LocalisationGeneralInfoDTO>>> GetLocalisationsGeneralInfoByNation(int nationId)
        {
            var localisations = await _context.Localisations
                .Where(l => l.NationId == nationId)
                .Select(l => new LocalisationGeneralInfoDTO
                {
                    Name = l.Name,
                    Size = l.Size,
                    Fortification = l.Fortification,
                    PopulationSize = _context.Populations.Where(p => p.Location.Id == l.Id).Count(),
                    PopulationHappiness = _context.Populations.Where(p => p.Location.Id == l.Id).Average(p => p.Happiness)
                })
                .ToListAsync();

            return Ok(localisations);
        }

        [HttpGet("{id}/Details")]
        public async Task<ActionResult<LocalisationDetailsDTO>> GetLocalisationDetails(int id)
        {
            var localisation = await _context.Localisations
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

        private List<LocalisationResourceInfoDTO> GetLocalisationResources(int localisationId)
        {
            var localisationList = _context.LocalisationResources
               .Where(lr => lr.LocationId == localisationId)
               .Select(lr => new LocalisationResourceInfoDTO
               {
                   Name = _context.Localisations
                       .Where(l => l.Id == lr.LocationId)
                       .Select(l => l.Name)
                       .FirstOrDefault(),
                   ResourceName = _context.Resources
                       .Where(r => r.Id == lr.ResourceId)
                       .Select(r => r.Name)
                       .FirstOrDefault(),
                   Amount = lr.Amount
               })
               .ToList();

            if (localisationList == null)
            {
                return new List<LocalisationResourceInfoDTO>();
            }
            else
            {
                return localisationList;
            }
        }
        private List<LocalisationResourceProductionDTO> GetLocalisationResourceProductions(int localisationId)
        {
            return _context.LocalisationResources
               .Where(lr => lr.LocationId == localisationId)
               .Select(lr => new LocalisationResourceProductionDTO
               {
                   ResourceName = _context.Resources
                       .Where(r => r.Id == lr.ResourceId)
                       .Select(r => r.Name)
                       .FirstOrDefault(),
                   Amount = CalculateProductionAmount(lr.ResourceId, localisationId)
               })
               .ToList();
        }

        private List<PopulationGroupDTO> GetPopulationGroups(int localisationId)
        {
            return _context.Populations
                           .Where(p => p.LocationId == localisationId)
                           .GroupBy(p => new { p.SocialGroupId, p.CultureId, p.ReligionId })
                           .Select(g => new PopulationGroupDTO
                           {
                               SocialGroup = _context.SocialGroups
                                   .Where(sg => sg.Id == g.Key.SocialGroupId)
                                   .Select(sg => sg.BaseHappiness.ToString())
                                   .FirstOrDefault(),
                               Culture = _context.Cultures
                                   .Where(c => c.Id == g.Key.CultureId)
                                   .Select(c => c.Id.ToString())
                                   .FirstOrDefault(),
                               Religion = _context.Religions
                                   .Where(r => r.Id == g.Key.ReligionId)
                                   .Select(r => r.Id.ToString())
                                   .FirstOrDefault(),
                               Amount = g.Count(),
                               Happiness = g.Average(p => p.Happiness)
                           })
                           .ToList();
        }

        private float CalculateProductionAmount(int resourceId, int localisationId)
        {
            var populationGroups = _context.Populations
                .Where(p => p.LocationId == localisationId)
                .GroupBy(p => p.SocialGroupId)
                .Select(g => new
                {
                    SocialGroupId = g.Key,
                    PopulationCount = g.Count()
                })
                .ToList();

            float totalProduction = 0;

            foreach (var group in populationGroups)
            {
                var productionShare = _context.ProductionShares
                    .Where(ps => ps.SocialGroupId == group.SocialGroupId && ps.ResourceId == resourceId)
                    .Select(ps => ps.Coefficient)
                    .FirstOrDefault();

                var resourceAmount = _context.LocalisationResources
                    .Where(lr => lr.LocationId == localisationId && lr.ResourceId == resourceId)
                    .Select(lr => lr.Amount)
                    .FirstOrDefault();

                totalProduction += group.PopulationCount * productionShare * resourceAmount;
            }

            return totalProduction;
        }
    }
}
