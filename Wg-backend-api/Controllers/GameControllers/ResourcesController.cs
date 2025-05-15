using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;
        public ResourcesController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;

            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        // GET: api/Resources
        // GET: api/Resources/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResources(int? id)
        {
            if (id.HasValue)
            {
                var resource = await _context.Resources.FindAsync(id);
                if (resource == null)
                {
                    return NotFound();
                }
                return Ok(new List<Resource> { resource });  // Zwraca pojedynczy zasób w liście
            }
            else
            {
                return await _context.Resources.ToListAsync();  // Zwraca wszystkie zasoby
            }
        }

        // PUT: api/Resources
        [HttpPut]
        public async Task<IActionResult> PutResources([FromBody] List<Resource> resources)
        {
            if (resources == null || resources.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var resource in resources)
            {
                _context.Entry(resource).State = EntityState.Modified;
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

        // POST: api/Resources
        [HttpPost]
        public async Task<ActionResult<Resource>> PostResources([FromBody] List<Resource> resources)
        {
            if (resources == null || resources.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }



            foreach (Resource resource in resources)
            {
                resource.Id = null;
            }
            _context.Resources.AddRange(resources);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResources", new { id = resources[0].Id }, resources);
        }

        // DELETE: api/Resources
        [HttpDelete]
        public async Task<ActionResult> DeleteResources([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var resources = await _context.Resources.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (resources.Count == 0)
            {
                return NotFound("Nie znaleziono zasobów do usunięcia.");
            }

            _context.Resources.RemoveRange(resources);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("nation/{nationId}/resource-balance")]
        public async Task<ActionResult<NationResourceBalanceDto>> GetNationResourceBalance(int nationId)
        {
            var resources = await GetAllResourcesAsync();
            var nation = await GetNationWithIncludesAsync(nationId);
            if (nation == null) return NotFound();

            var acceptedTradeAgreements = await GetAcceptedTradeAgreementsAsync(nationId);

            var result = new NationResourceBalanceDto
            {
                Resources = resources,
                ResourceBalances = new List<ResourceBalanceDto>()
            };

            foreach (var resource in resources)
            {
                var resourceBalance = new ResourceBalanceDto
                {
                    ResourceId = resource.Id,
                    CurrentAmount = GetCurrentResourceAmount(nation, resource.Id),
                    ArmyMaintenanceExpenses = GetArmyMaintenanceExpenses(nation, resource.Id),
                    PopulationExpenses = GetPopulationExpenses(nation, resource.Id),
                    PopulationProduction = GetPopulationProduction(nation, resource.Id),
                    TradeIncome = GetTradeIncome(acceptedTradeAgreements, nationId, resource.Id),
                    TradeExpenses = GetTradeExpenses(acceptedTradeAgreements, nationId, resource.Id),
                    EventBalance = 0
                };

                resourceBalance.TotalBalance =
                    resourceBalance.PopulationProduction
                    + resourceBalance.TradeIncome
                    + resourceBalance.EventBalance
                    - resourceBalance.ArmyMaintenanceExpenses
                    - resourceBalance.PopulationExpenses
                    - resourceBalance.TradeExpenses;

                result.ResourceBalances.Add(resourceBalance);
            }

            return Ok(result);
        }

        // --- PODFUNKCJE ---

        private async Task<List<ResourceDto>> GetAllResourcesAsync()
        {
            return await _context.Resources
                .AsNoTracking()
                .Select(r => new ResourceDto
                {
                    Id = r.Id.Value,
                    Name = r.Name,
                    IsMain = r.IsMain
                })
                .ToListAsync();
        }

        private async Task<Nation> GetNationWithIncludesAsync(int nationId)
        {
            return await _context.Nations
                .AsNoTracking()
                .Where(n => n.Id == nationId)
                .Include(n => n.Localisations)
                    .ThenInclude(l => l.LocalisationResources)
                .Include(n => n.Localisations)
                    .ThenInclude(l => l.Populations)
                        .ThenInclude(p => p.SocialGroup)
                            .ThenInclude(sg => sg.UsedResources)
                .Include(n => n.Localisations)
                    .ThenInclude(l => l.Populations)
                        .ThenInclude(p => p.SocialGroup)
                            .ThenInclude(sg => sg.ProductionShares)
                .Include(n => n.Armies)
                    .ThenInclude(a => a.Troops)
                        .ThenInclude(t => t.UnitType)
                            .ThenInclude(ut => ut.MaintenaceCosts)
                .Include(n => n.RelatedEvents)
                    .ThenInclude(re => re.Event)
                        .ThenInclude(e => e.Modifiers)
                .FirstOrDefaultAsync();
        }

        private async Task<List<TradeAgreement>> GetAcceptedTradeAgreementsAsync(int nationId)
        {
            return await _context.TradeAgreements
                .Where(ta => ta.isAccepted && (ta.OferingNationId == nationId || ta.ReceivingNationId == nationId))
                .Include(ta => ta.OfferedResources)
                .Include(ta => ta.WantedResources)
                .ToListAsync();
        }

        private float GetCurrentResourceAmount(Nation nation, int resourceId)
        {
            return nation.Localisations
                .SelectMany(l => l.LocalisationResources)
                .Where(lr => lr.ResourceId == resourceId)
                .Sum(lr => lr.Amount);
        }

        private float GetArmyMaintenanceExpenses(Nation nation, int resourceId)
        {
            return nation.Armies
                .SelectMany(a => a.Troops)
                .SelectMany(t => t.UnitType.MaintenaceCosts
                    .Where(mc => mc.ResourceId == resourceId)
                    .Select(mc => mc.Amount * t.Quantity))
                .Sum();
        }

        private float GetPopulationExpenses(Nation nation, int resourceId)
        {
            return nation.Localisations
                .SelectMany(l => l.Populations)
                .SelectMany(p => p.SocialGroup.UsedResources
                    .Where(ur => ur.ResourceId == resourceId)
                    .Select(ur => ur.Amount))
                .Sum();
        }

        private float GetPopulationProduction(Nation nation, int resourceId)
        {
            return nation.Localisations
                .SelectMany(l => l.Populations)
                .SelectMany(p => p.SocialGroup.ProductionShares
                    .Where(ps => ps.ResourceId == resourceId)
                    .Select(ps => ps.Coefficient))
                .Sum();
        }

        private int GetTradeIncome(List<TradeAgreement> tradeAgreements, int nationId, int resourceId)
        {
            int tradeIncome = 0;
            foreach (var ta in tradeAgreements)
            {
                bool isOffering = ta.OferingNationId == nationId;
                if (isOffering)
                {
                    // Chciane zasoby to przychód
                    tradeIncome += ta.WantedResources
                        .Where(wr => wr.ResourceId == resourceId)
                        .Sum(wr => wr.Amount);
                }
                else
                {
                    // Oferowane zasoby to przychód
                    tradeIncome += ta.OfferedResources
                        .Where(or => or.ResourceId == resourceId)
                        .Sum(or => or.Quantity);
                }
            }
            return tradeIncome;
        }

        private int GetTradeExpenses(List<TradeAgreement> tradeAgreements, int nationId, int resourceId)
        {
            int tradeExpenses = 0;
            foreach (var ta in tradeAgreements)
            {
                bool isOffering = ta.OferingNationId == nationId;
                if (isOffering)
                {
                    // Oferowane zasoby to wydatek
                    tradeExpenses += ta.OfferedResources
                        .Where(or => or.ResourceId == resourceId)
                        .Sum(or => or.Quantity);
                }
                else
                {
                    // Chciane zasoby to wydatek
                    tradeExpenses += ta.WantedResources
                        .Where(wr => wr.ResourceId == resourceId)
                        .Sum(wr => wr.Amount);
                }
            }
            return tradeExpenses;
        }

        //private int GetEventBalance(Nation nation, int resourceId)
        //{
        //    return nation.RelatedEvents
        //        .SelectMany(re => re.Event.Modifiers
        //            .Where(m => m.ResourceId == resourceId && m.modifireType == ModifireType.Value)
        //            .Select(m => m.Amount))
        //        .Sum();
        //}


    }
}
