using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int _nationId;

        public ResourcesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
            _nationId = int.Parse(nationIdStr);
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
        public async Task<ActionResult<List<ResourceDto>>> PostResources([FromForm] List<CreateResourceDto> resources)
        {
            if (resources == null || !resources.Any())
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            const int maxFileSize = 5 * 1024 * 1024;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "resources", "icons");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var savedResources = new List<Resource>();

            foreach (var dto in resources)
            {
                string? iconPath = null;

                if (dto.IconFile != null)
                {
                    var extension = Path.GetExtension(dto.IconFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                        return BadRequest($"Nieprawidłowe rozszerzenie pliku: {dto.IconFile.FileName}");

                    if (dto.IconFile.Length > maxFileSize)
                        return BadRequest($"Plik {dto.IconFile.FileName} przekracza maksymalny rozmiar {maxFileSize / 1024 / 1024} MB");

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.IconFile.CopyToAsync(stream);
                    }

                    iconPath = $"/images/{uniqueFileName}";
                }

                savedResources.Add(new Resource
                {
                    Name = dto.Name,
                    IsMain = dto.IsMain,
                    Icon = iconPath
                });
            }

            _context.Resources.AddRange(savedResources);
            await _context.SaveChangesAsync();

            var response = savedResources.Select(r => new ResourceDto
            {
                Id = (int)r.Id,
                Name = r.Name,
                IsMain = r.IsMain,
                Icon = r.Icon != null ? $"{Request.Scheme}://{Request.Host}/{r.Icon}" : null
            }).ToList();

            return CreatedAtAction(nameof(PostResources), null, response);
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

        [HttpGet("nation/{nationId?}/resource-balance")]
        public async Task<ActionResult<NationResourceBalanceDto>> GetNationResourceBalance(int? nationId)
        {

            if (!nationId.HasValue)
            {
                nationId = _nationId;
            }


            var resources = await GetAllResourcesAsync();
            var nation = await GetNationWithIncludesAsync((int)nationId);
            if (nation == null) return NotFound();

            var acceptedTradeAgreements = await GetAcceptedTradeAgreementsAsync((int)nationId);

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
                    TradeIncome = GetTradeIncome(acceptedTradeAgreements, (int)nationId, resource.Id),
                    TradeExpenses = GetTradeExpenses(acceptedTradeAgreements,(int) nationId, resource.Id),
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

        private float GetTradeIncome(List<TradeAgreement> tradeAgreements, int nationId, int resourceId)
        {
            float tradeIncome = 0;
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

        private float GetTradeExpenses(List<TradeAgreement> tradeAgreements, int nationId, int resourceId)
        {
            float tradeExpenses = 0;
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

        [HttpGet("nation/{nationId?}/owned-resources")]
        public async Task<ActionResult<List<ResourceAmountDto>>> GetOwnedResources(int? nationId)
        {

            if(nationId == null)
            {
                nationId = _nationId;
            }

            var nation = await _context.Nations
                .AsNoTracking()
                .Where(n => n.Id == nationId)
                .Include(n => n.Localisations)
                    .ThenInclude(l => l.LocalisationResources)
                        .ThenInclude(lr => lr.Resource)
                .FirstOrDefaultAsync();

            if (nation == null)
            {
                return NotFound($"Nie znaleziono państwa o ID: {nationId}");
            }

            var ownedResources = nation.Localisations
                .SelectMany(l => l.LocalisationResources)
                .GroupBy(lr => new { lr.ResourceId, lr.Resource.Name })
                .Select(g => new ResourceAmountDto
                {
                    ResourceId = g.Key.ResourceId,
                    ResourceName = g.Key.Name,
                    Amount = g.Sum(lr => lr.Amount)
                })
                .ToList();

            return Ok(ownedResources);
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
