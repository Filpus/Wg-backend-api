using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocialGroupsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public SocialGroupsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        // GET: api/SocialGroups
        // GET: api/SocialGroups/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<SocialGroupDTO>>> GetSocialGroups(int? id)
        {
            if (id.HasValue)
            {
                var socialGroup = await _context.SocialGroups.FindAsync(id);
                if (socialGroup == null)
                {
                    return NotFound();
                }
                var socialGroupDTO = new SocialGroupDTO
                {
                    Id = socialGroup.Id,
                    Name = socialGroup.Name,
                    BaseHappiness = socialGroup.BaseHappiness,
                    Volunteers = socialGroup.Volunteers
                };
                return Ok(new List<SocialGroupDTO> { socialGroupDTO });
            }
            else
            {
                var socialGroups = await _context.SocialGroups.ToListAsync();
                var socialGroupDTOs = socialGroups.Select(sg => new SocialGroupDTO
                {
                    Id = sg.Id,
                    Name = sg.Name,
                    BaseHappiness = sg.BaseHappiness,
                    Volunteers = sg.Volunteers
                }).ToList();
                return Ok(socialGroupDTOs);
            }
        }

        // PUT: api/SocialGroups
        [HttpPut]
        public async Task<IActionResult> PutSocialGroups([FromBody] List<SocialGroupDTO> socialGroupDTOs)
        {
            if (socialGroupDTOs == null || socialGroupDTOs.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var socialGroupDTO in socialGroupDTOs)
            {
                var socialGroup = await _context.SocialGroups.FindAsync(socialGroupDTO.Id);
                if (socialGroup == null)
                {
                    return NotFound($"Nie znaleziono grupy społecznej o ID {socialGroupDTO.Id}.");
                }
                socialGroup.Name = socialGroupDTO.Name;
                socialGroup.BaseHappiness = socialGroupDTO.BaseHappiness;
                socialGroup.Volunteers = socialGroupDTO.Volunteers;
                _context.Entry(socialGroup).State = EntityState.Modified;
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

        // POST: api/SocialGroups
        [HttpPost]
        public async Task<ActionResult<IEnumerable<SocialGroupDTO>>> PostSocialGroups([FromBody] List<SocialGroupDTO> socialGroupDTOs)
        {
            if (socialGroupDTOs == null || socialGroupDTOs.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var socialGroups = socialGroupDTOs.Select(dto => new SocialGroup
            {
                Name = dto.Name,
                BaseHappiness = dto.BaseHappiness,
                Volunteers = dto.Volunteers
            }).ToList();

            _context.SocialGroups.AddRange(socialGroups);
            await _context.SaveChangesAsync();

            var createdDTOs = socialGroups.Select(sg => new SocialGroupDTO
            {
                Id = sg.Id,
                Name = sg.Name,
                BaseHappiness = sg.BaseHappiness,
                Volunteers = sg.Volunteers
            }).ToList();

            return CreatedAtAction("GetSocialGroups", new { id = createdDTOs[0].Id }, createdDTOs);
        }

        // DELETE: api/SocialGroups
        [HttpDelete]
        public async Task<ActionResult> DeleteSocialGroups([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var socialGroups = await _context.SocialGroups.Where(s => ids.Contains(s.Id)).ToListAsync();

            if (socialGroups.Count == 0)
            {
                return NotFound("Nie znaleziono grup społecznych do usunięcia.");
            }

            _context.SocialGroups.RemoveRange(socialGroups);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("info")]
        public async Task<ActionResult<IEnumerable<SocialGroupInfoDTO>>> GetSocialGroupInfo()
        {
            var socialGroups = await _context.SocialGroups
                .Include(sg => sg.UsedResources)
                    .ThenInclude(ur => ur.Resource)
                .Include(sg => sg.ProductionShares)
                    .ThenInclude(ps => ps.Resource)
                .ToListAsync();

            var socialGroupInfoList = socialGroups?.Select(sg => new SocialGroupInfoDTO
            {
                Name = sg.Name,
                BaseHappiness = CalculateBaseHappiness(sg),
                Volunteers = CalculateVolunteers(sg),
                ConsumedResources = GetConsumedResources(sg),
                ProducedResources = GetProducedResources(sg)
            }).ToList() ?? new List<SocialGroupInfoDTO>();

            return Ok(socialGroupInfoList);
        }

        private float CalculateBaseHappiness(SocialGroup socialGroup)
        {
            // Placeholder logic for calculating base happiness
            return socialGroup.BaseHappiness;
        }

        private int CalculateVolunteers(SocialGroup socialGroup)
        {
            // Placeholder logic for calculating volunteers
            return socialGroup.Volunteers;
        }

        private List<ResourceAmountDto> GetConsumedResources(SocialGroup socialGroup)
        {
            return socialGroup.UsedResources.Select(ur => new ResourceAmountDto
            {
                ResourceName = ur.Resource.Name,
                ResourceId = ur.ResourceId,
                Amount = ur.Amount // Use the actual amount from UsedResources
            }).ToList();
        }

        private List<ResourceAmountDto> GetProducedResources(SocialGroup socialGroup)
        {
            return socialGroup.ProductionShares.Select(ps => new ResourceAmountDto
            {
                ResourceName = ps.Resource.Name,
                ResourceId = ps.ResourceId,
                Amount = ps.Coefficient // Calculate based on Coeficence
            }).ToList();
        }

    }
}
