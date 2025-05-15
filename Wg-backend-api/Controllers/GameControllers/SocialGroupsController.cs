using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocialGroupsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;
        public SocialGroupsController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;

            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }
        // GET: api/SocialGroups
        // GET: api/SocialGroups/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<SocialGroup>>> GetSocialGroups(int? id)
        {
            if (id.HasValue)
            {
                var socialGroup = await _context.SocialGroups.FindAsync(id);
                if (socialGroup == null)
                {
                    return NotFound();
                }
                return Ok(new List<SocialGroup> { socialGroup });
            }
            else
            {
                return await _context.SocialGroups.ToListAsync();
            }
        }

        // PUT: api/SocialGroups
        [HttpPut]
        public async Task<IActionResult> PutSocialGroups([FromBody] List<SocialGroup> socialGroups)
        {
            if (socialGroups == null || socialGroups.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var socialGroup in socialGroups)
            {
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
        public async Task<ActionResult<SocialGroup>> PostSocialGroups([FromBody] List<SocialGroup> socialGroups)
        {
            if (socialGroups == null || socialGroups.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }
            foreach (SocialGroup group in socialGroups)
            {
                group.Id = null;
            }

            _context.SocialGroups.AddRange(socialGroups);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSocialGroups", new { id = socialGroups[0].Id }, socialGroups);
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
                .Include(sg => sg.ProductionShares)
                .ToListAsync();

            var socialGroupInfoList = socialGroups.Select(sg => new SocialGroupInfoDTO
            {
                Name = sg.Name,
                BaseHappiness = CalculateBaseHappiness(sg),
                Volunteers = CalculateVolunteers(sg),
                ConsumedResources = GetConsumedResources(sg),
                ProducedResources = GetProducedResources(sg)
            }).ToList();

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
