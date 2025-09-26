using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PopulationsController : ControllerBase
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int? _nationId;

        public PopulationsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        // GET: api/Populations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PopulationDTO>>> GetPopulation()
        {
            return await _context.Populations
                .Select(p => new PopulationDTO
                {
                    Id = p.Id,
                    ReligionId = p.ReligionId,
                    CultureId = p.CultureId,
                    SocialGroupId = p.SocialGroupId,
                    LocationId = p.LocationId,
                    Happiness = p.Happiness
                })
                .ToListAsync();
        }

        // GET: api/Populations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PopulationDTO>> GetPopulation(int? id)
        {
            var population = await _context.Populations
                .Where(p => p.Id == id)
                .Select(p => new PopulationDTO
                {
                    Id = p.Id,
                    ReligionId = p.ReligionId,
                    CultureId = p.CultureId,
                    SocialGroupId = p.SocialGroupId,
                    LocationId = p.LocationId,
                    Happiness = p.Happiness
                })
                .FirstOrDefaultAsync();

            if (population == null)
            {
                return NotFound();
            }

            return population;
        }

        // PUT: api/Populations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPopulation(PopulationDTO[] populations)
        {
            if (populations == null || populations.Length == 0)
            {
                return BadRequest();
            }

            foreach (var popDto in populations)
            {
                var population = await _context.Populations.FindAsync(popDto.Id);
                if (population == null)
                {
                    return NotFound();
                }

                population.ReligionId = popDto.ReligionId;
                population.CultureId = popDto.CultureId;
                population.SocialGroupId = popDto.SocialGroupId;
                population.LocationId = popDto.LocationId;
                population.Happiness = popDto.Happiness;

                _context.Entry(population).State = EntityState.Modified;
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

        // POST: api/Populations
        [HttpPost]
        public async Task<ActionResult<IEnumerable<PopulationDTO>>> PostPopulation(List<PopulationDTO> populationDtos)
        {
            if (populationDtos == null || populationDtos.Count == 0)
            {
                return BadRequest();
            }

            var createdDtos = new List<PopulationDTO>();

            foreach (var populationDto in populationDtos)
            {

                    var socialGroup = await _context.SocialGroups.FirstOrDefaultAsync(sg => sg.Id == populationDto.SocialGroupId);
                    if (socialGroup == null)
                    {
                        return BadRequest($"Nie znaleziono grupy społecznej o ID {populationDto.SocialGroupId}");
                    }

                    var population = new Population
                    {
                        ReligionId = populationDto.ReligionId,
                        CultureId = populationDto.CultureId,
                        SocialGroupId = populationDto.SocialGroupId,
                        LocationId = populationDto.LocationId,
                        Happiness = socialGroup.BaseHappiness,
                        Volunteers = socialGroup.Volunteers,
                    };

                    _context.Populations.Add(population);
                    await _context.SaveChangesAsync();

                    populationDto.Id = population.Id;
                    populationDto.Happiness = population.Happiness;
                    populationDto.Volonteers = population.Volunteers;
                    createdDtos.Add(populationDto);
                }



            return CreatedAtAction("GetPopulation", createdDtos);
        }

        // DELETE: api/Populations/5
        [HttpDelete]
        public async Task<IActionResult> DeletePopulations([FromBody] int[] ids)
        {
            foreach (var id in ids)
            {
                var population = await _context.Populations.FindAsync(id);
                if (population == null)
                {
                    return NotFound();
                }

                _context.Populations.Remove(population);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool PopulationExists(int? id)
        {
            return _context.Populations.Any(e => e.Id == id);
        }

        [HttpGet("nation/population-groups/{nationId?}")]
        public async Task<ActionResult<IEnumerable<PopulationGroupDTO>>> GetPopulationGroupsByNation(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var populationGroups = await _context.Populations
                .Where(p => _context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .GroupBy(p => new { p.ReligionId, p.CultureId, p.SocialGroupId })
                .Select(g => new PopulationGroupDTO
                {
                    Religion = _context.Religions.FirstOrDefault(r => r.Id == g.Key.ReligionId).Name,
                    Culture = _context.Cultures.FirstOrDefault(c => c.Id == g.Key.CultureId).Name,
                    SocialGroup = _context.SocialGroups.FirstOrDefault(s => s.Id == g.Key.SocialGroupId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();

            return Ok(populationGroups);
        }

        [HttpGet("nation/population-culture-groups/{nationId?}")]
        public async Task<ActionResult<IEnumerable<PopulationCultureGroupDTO>>> GetPopulationCultureGroupsByNation(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var populationGroups = await _context.Populations
                .Where(p => _context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .GroupBy(p => new { p.CultureId })
                .Select(g => new PopulationCultureGroupDTO
                {
                    Culture = _context.Cultures.FirstOrDefault(c => c.Id == g.Key.CultureId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }

        [HttpGet("nation/population-social-groups/{nationId?}")]
        public async Task<ActionResult<IEnumerable<PopulationSocialGroupDTO>>> GetPopulationSocialGroupsByNation(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var populationGroups = await _context.Populations
                .Where(p => _context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .GroupBy(p => new { p.SocialGroupId })
                .Select(g => new PopulationSocialGroupDTO
                {
                    SocialGroup = _context.SocialGroups.FirstOrDefault(s => s.Id == g.Key.SocialGroupId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }
        [HttpGet("nation/population-religion-groups/{nationId?}")]
        public async Task<ActionResult<IEnumerable<PopulationReligiousGroupDTO>>> GetPopulationReligiousGroupsByNation(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var populationGroups = await _context.Populations
                .Where(p => _context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .GroupBy(p => new { p.ReligionId })
                .Select(g => new PopulationReligiousGroupDTO
                {
                    Religion = _context.Religions.FirstOrDefault(r => r.Id == g.Key.ReligionId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }



        [HttpGet("location/population-groups/{locationId}")]
        public async Task<ActionResult<IEnumerable<PopulationGroupDTO>>> GetPopulationGroupsByLocation(int locationId)
        {

            var populationGroups = await _context.Populations
                .Where(p => p.LocationId == locationId)
                .GroupBy(p => new { p.ReligionId, p.CultureId, p.SocialGroupId })
                .Select(g => new PopulationGroupDTO
                {
                    Religion = _context.Religions.FirstOrDefault(r => r.Id == g.Key.ReligionId).Name,
                    Culture = _context.Cultures.FirstOrDefault(c => c.Id == g.Key.CultureId).Name,
                    SocialGroup = _context.SocialGroups.FirstOrDefault(s => s.Id == g.Key.SocialGroupId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();

            return Ok(populationGroups);
        }

        [HttpGet("location/population-culture-groups/{locationId}")]
        public async Task<ActionResult<IEnumerable<PopulationCultureGroupDTO>>> GetPopulationCultureGroupsByLocation(int locationId)
        {
            var populationGroups = await _context.Populations
                .Where(p => p.LocationId == locationId)
                .GroupBy(p => new { p.CultureId })
                .Select(g => new PopulationCultureGroupDTO
                {
                    Culture = _context.Cultures.FirstOrDefault(c => c.Id == g.Key.CultureId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }
        [HttpGet("location/population-social-groups/{locationId}")]
        public async Task<ActionResult<IEnumerable<PopulationSocialGroupDTO>>> GetPopulationSocialGroupsByLocation(int locationId)
        {
            var populationGroups = await _context.Populations
                .Where(p => p.LocationId == locationId)
                .GroupBy(p => new { p.SocialGroupId })
                .Select(g => new PopulationSocialGroupDTO
                {
                    SocialGroup = _context.SocialGroups.FirstOrDefault(s => s.Id == g.Key.SocialGroupId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }
        [HttpGet("location/population-religion-groups/{locationId}")]
        public async Task<ActionResult<IEnumerable<PopulationReligiousGroupDTO>>> GetPopulationReligiousGroupsByLocation(int locationId)
        {
            var populationGroups = await _context.Populations
                .Where(p => p.LocationId == locationId)
                .GroupBy(p => new { p.ReligionId })
                .Select(g => new PopulationReligiousGroupDTO
                {
                    Religion = _context.Religions.FirstOrDefault(r => r.Id == g.Key.ReligionId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }

        [HttpGet("nation/total-population-info/{nationId?}")]
        public async Task<ActionResult<TotalPopulationInfoDTO>> GetTotalPopulationInfo(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var totalPopulation = await _context.Populations
                .Where(p => _context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .CountAsync();
            var averageHappiness = await _context.Populations
                .Where(p => _context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .AverageAsync(p => p.Happiness);
            return Ok(new TotalPopulationInfoDTO
            {
                TotalPopulation = totalPopulation,
                AverageHappiness = averageHappiness
            });
        }
    }
}
