using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class PopulationsController : GameControllerBase
    {
        public PopulationsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        // GET: api/Populations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PopulationDTO>>> GetPopulation()
        {
            return await this.Context.Populations
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
            var population = await this.Context.Populations
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
                var population = await this.Context.Populations.FindAsync(popDto.Id);
                if (population == null)
                {
                    return NotFound();
                }

                population.ReligionId = popDto.ReligionId;
                population.CultureId = popDto.CultureId;
                population.SocialGroupId = popDto.SocialGroupId;
                population.LocationId = popDto.LocationId;
                population.Happiness = popDto.Happiness;

                this.Context.Entry(population).State = EntityState.Modified;
            }

            try
            {
                await this.Context.SaveChangesAsync();
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

                var socialGroup = await this.Context.SocialGroups.FirstOrDefaultAsync(sg => sg.Id == populationDto.SocialGroupId);
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

                this.Context.Populations.Add(population);
                await this.Context.SaveChangesAsync();

                populationDto.Id = population.Id;
                populationDto.Happiness = population.Happiness;
                populationDto.Volonteers = population.Volunteers;
                createdDtos.Add(populationDto);
            }

            return CreatedAtAction("GetPopulation", createdDtos);
        }

        // DELETE: api/Populations/5
        [HttpDelete]
        public async Task<IActionResult> DeletePopulations([FromBody] List<PopulationGroupDTO> groups)
        {
            if (groups == null || groups.Count == 0)
            {
                return BadRequest();
            }

            using var transaction = await this.Context.Database.BeginTransactionAsync();

            var deletionResults = new List<object>();

            foreach (var group in groups)
            {
                if (group == null || group.Amount <= 0)
                {
                    deletionResults.Add(new
                    {
                        group?.ReligionId,
                        group?.CultureId,
                        group?.SocialGroupId,
                        Requested = group?.Amount ?? 0,
                        Deleted = 0
                    });
                    continue;
                }

                var query = this.Context.Populations
                    .Where(p =>
                        p.ReligionId == group.ReligionId &&
                        p.CultureId == group.CultureId &&
                        p.SocialGroupId == group.SocialGroupId)
                    .OrderBy(p => p.Id);

                var toDelete = await query.Take(group.Amount).ToListAsync();

                if (toDelete.Count > 0)
                {
                    this.Context.Populations.RemoveRange(toDelete);
                }

                deletionResults.Add(new
                {
                    group.ReligionId,
                    group.CultureId,
                    group.SocialGroupId,
                    Requested = group.Amount,
                    Deleted = toDelete.Count
                });
            }

            try
            {
                await this.Context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Błąd podczas usuwania populacji.");
            }

            return Ok(deletionResults);
        }

        [HttpGet("nation/population-groups/{nationId?}")]
        public async Task<ActionResult<IEnumerable<PopulationGroupDTO>>> GetPopulationGroupsByNation(int? nationId)
        {
            nationId ??= this.NationId;
            var populationGroups = await this.Context.Populations
                .Where(p => this.Context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .GroupBy(p => new { p.ReligionId, p.CultureId, p.SocialGroupId })
                .Select(g => new PopulationGroupDTO
                {
                    ReligionId = g.Key.ReligionId,
                    CultureId = g.Key.CultureId,
                    SocialGroupId = g.Key.SocialGroupId,
                    Religion = this.Context.Religions.Where(r => r.Id == g.Key.ReligionId).Select(r => r.Name).FirstOrDefault() ?? string.Empty,
                    Culture = this.Context.Cultures.Where(c => c.Id == g.Key.CultureId).Select(c => c.Name).FirstOrDefault() ?? string.Empty,
                    SocialGroup = this.Context.SocialGroups.Where(s => s.Id == g.Key.SocialGroupId).Select(s => s.Name).FirstOrDefault() ?? string.Empty,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness),
                })
                .ToListAsync();

            return Ok(populationGroups);
        }

        [HttpGet("nation/population-culture-groups/{nationId?}")]
        public async Task<ActionResult<IEnumerable<PopulationCultureGroupDTO>>> GetPopulationCultureGroupsByNation(int? nationId)
        {
            nationId ??= this.NationId;
            var populationGroups = await this.Context.Populations
                .Where(p => this.Context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .GroupBy(p => new { p.CultureId })
                .Select(g => new PopulationCultureGroupDTO
                {
                    Culture = this.Context.Cultures.FirstOrDefault(c => c.Id == g.Key.CultureId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }

        [HttpGet("nation/population-social-groups/{nationId?}")]
        public async Task<ActionResult<IEnumerable<PopulationSocialGroupDTO>>> GetPopulationSocialGroupsByNation(int? nationId)
        {
            nationId ??= this.NationId;
            var populationGroups = await this.Context.Populations
                .Where(p => this.Context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .GroupBy(p => new { p.SocialGroupId })
                .Select(g => new PopulationSocialGroupDTO
                {
                    SocialGroup = this.Context.SocialGroups.FirstOrDefault(s => s.Id == g.Key.SocialGroupId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }
        [HttpGet("nation/population-religion-groups/{nationId?}")]
        public async Task<ActionResult<IEnumerable<PopulationReligiousGroupDTO>>> GetPopulationReligiousGroupsByNation(int? nationId)
        {
            nationId ??= this.NationId;
            var populationGroups = await this.Context.Populations
                .Where(p => this.Context.Localisations.Any(l => l.Id == p.LocationId && l.NationId == nationId))
                .GroupBy(p => new { p.ReligionId })
                .Select(g => new PopulationReligiousGroupDTO
                {
                    Religion = this.Context.Religions.FirstOrDefault(r => r.Id == g.Key.ReligionId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }

        [HttpGet("location/population-groups/{locationId}")]
        public async Task<ActionResult<IEnumerable<PopulationGroupDTO>>> GetPopulationGroupsByLocation(int locationId)
        {

            var populationGroups = await this.Context.Populations
                .Where(p => p.LocationId == locationId)
                .GroupBy(p => new { p.ReligionId, p.CultureId, p.SocialGroupId })
                .Select(g => new PopulationGroupDTO
                {
                    ReligionId = g.Key.ReligionId,
                    CultureId = g.Key.CultureId,
                    SocialGroupId = g.Key.SocialGroupId,
                    Religion = this.Context.Religions.Where(r => r.Id == g.Key.ReligionId).Select(r => r.Name).FirstOrDefault() ?? string.Empty,
                    Culture = this.Context.Cultures.Where(c => c.Id == g.Key.CultureId).Select(c => c.Name).FirstOrDefault() ?? string.Empty,
                    SocialGroup = this.Context.SocialGroups.Where(s => s.Id == g.Key.SocialGroupId).Select(s => s.Name).FirstOrDefault() ?? string.Empty,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();

            return Ok(populationGroups);
        }

        [HttpGet("location/population-culture-groups/{locationId}")]
        public async Task<ActionResult<IEnumerable<PopulationCultureGroupDTO>>> GetPopulationCultureGroupsByLocation(int locationId)
        {
            var populationGroups = await this.Context.Populations
                .Where(p => p.LocationId == locationId)
                .GroupBy(p => new { p.CultureId })
                .Select(g => new PopulationCultureGroupDTO
                {
                    Culture = this.Context.Cultures.FirstOrDefault(c => c.Id == g.Key.CultureId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }
        [HttpGet("location/population-social-groups/{locationId}")]
        public async Task<ActionResult<IEnumerable<PopulationSocialGroupDTO>>> GetPopulationSocialGroupsByLocation(int locationId)
        {
            var populationGroups = await this.Context.Populations
                .Where(p => p.LocationId == locationId)
                .GroupBy(p => new { p.SocialGroupId })
                .Select(g => new PopulationSocialGroupDTO
                {
                    SocialGroup = this.Context.SocialGroups.FirstOrDefault(s => s.Id == g.Key.SocialGroupId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }
        [HttpGet("location/population-religion-groups/{locationId}")]
        public async Task<ActionResult<IEnumerable<PopulationReligiousGroupDTO>>> GetPopulationReligiousGroupsByLocation(int locationId)
        {
            var populationGroups = await this.Context.Populations
                .Where(p => p.LocationId == locationId)
                .GroupBy(p => new { p.ReligionId })
                .Select(g => new PopulationReligiousGroupDTO
                {
                    Religion = this.Context.Religions.FirstOrDefault(r => r.Id == g.Key.ReligionId).Name,
                    Amount = g.Count(),
                    Happiness = g.Average(p => p.Happiness)
                })
                .ToListAsync();
            return Ok(populationGroups);
        }

        [HttpGet("nation/total-population-info/{nationId?}")]
        public async Task<ActionResult<TotalPopulationInfoDTO>> GetTotalPopulationInfo(int? nationId)
        {
            nationId ??= this.NationId;
            var populationQuery = this.Context.Populations
                .Where(p => this.Context.Localisations
                    .Any(l => l.Id == p.LocationId && l.NationId == nationId));

            var totalPopulation = await populationQuery.CountAsync();

            double averageHappiness = 0;
            if (totalPopulation > 0)
            {
                averageHappiness = await populationQuery.AverageAsync(p => p.Happiness);
            }

            return Ok(new TotalPopulationInfoDTO
            {
                TotalPopulation = totalPopulation,
                AverageHappiness = (float)averageHappiness,
            });
        }
    }
}
