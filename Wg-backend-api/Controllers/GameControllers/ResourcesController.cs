namespace Wg_backend_api.Controllers.GameControllers
{
    // For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Wg_backend_api.Data;
    using Wg_backend_api.DTO;
    using Wg_backend_api.Logic.Modifiers.Processors;
    using Wg_backend_api.Models;
    using Wg_backend_api.Services;

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
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = this._gameDbContextFactory.Create(schema);
            string nationIdStr = this._sessionDataService.GetNation();
            this._nationId = int.Parse(nationIdStr);
        }

        // GET: api/Resources
        // GET: api/Resources/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<ResourceDto>>> GetResources(int? id)
        {
            if (id.HasValue)
            {
                var resource = await this._context.Resources.FindAsync(id);
                if (resource == null)
                {
                    return this.NotFound();
                }

                var resourceDto = new ResourceDto
                {
                    Id = resource.Id.Value,
                    Name = resource.Name,
                    IsMain = resource.IsMain,
                    Icon = resource.Icon,
                };
                return this.Ok(new List<ResourceDto> { resourceDto }); // Zwraca pojedynczy zasób w liście
            }
            else
            {
                var resources = await this._context.Resources.ToListAsync();
                var resourceDtos = resources.Select(r => new ResourceDto
                {
                    Id = r.Id.Value,
                    Name = r.Name,
                    IsMain = r.IsMain,
                    Icon = r.Icon,
                }).ToList();
                return this.Ok(resourceDtos); // Zwraca wszystkie zasoby
            }
        }

        // PUT: api/Resources
        [HttpPut]
        public async Task<IActionResult> PutResources([FromBody] List<ResourceDto> resourceDtos)
        {
            if (resourceDtos == null || resourceDtos.Count == 0)
            {
                return this.BadRequest("Brak danych do edycji.");
            }

            // check the name is between 1 and 64 characters
            foreach (var resourceDto in resourceDtos)
            {
                if (string.IsNullOrWhiteSpace(resourceDto.Name) || resourceDto.Name.Length > 64)
                {
                    return this.BadRequest("Nazwa zasobu jest niepoprawnej długości.");
                }
            }

            foreach (var resourceDto in resourceDtos)
            {
                var resource = await this._context.Resources.FindAsync(resourceDto.Id);
                if (resource == null)
                {
                    return this.NotFound($"Nie znaleziono zasobu o ID: {resourceDto.Id}");
                }

                resource.Name = resourceDto.Name;
                resource.IsMain = resourceDto.IsMain;
                resource.Icon = resourceDto.Icon;

                this._context.Entry(resource).State = EntityState.Modified;
            }

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return this.StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        // POST: api/Resources
        [HttpPost]
        public async Task<ActionResult<List<ResourceDto>>> PostResources([FromForm] List<CreateResourceDto> resources)
        {
            if (resources == null || !resources.Any())
            {
                return this.BadRequest("Brak danych do zapisania.");
            }

            // check the name is between 1 and 64 characters
            foreach (var dto in resources)
            {
                if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 64)
                {
                    return this.BadRequest("Nazwa zasobu jest niepoprawnej długości.");
                }
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            const int maxFileSize = 5 * 1024 * 1024;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "resources", "icons");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var savedResources = new List<Resource>();

            foreach (var dto in resources)
            {
                string? iconPath = null;

                if (dto.IconFile != null)
                {
                    var extension = Path.GetExtension(dto.IconFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return this.BadRequest($"Nieprawidłowe rozszerzenie pliku: {dto.IconFile.FileName}");
                    }

                    if (dto.IconFile.Length > maxFileSize)
                    {
                        return this.BadRequest($"Plik {dto.IconFile.FileName} przekracza maksymalny rozmiar {maxFileSize / 1024 / 1024} MB");
                    }

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

            this._context.Resources.AddRange(savedResources);
            await this._context.SaveChangesAsync();

            var response = savedResources.Select(r => new ResourceDto
            {
                Id = (int)r.Id,
                Name = r.Name,
                IsMain = r.IsMain,
                Icon = r.Icon != null ? $"{this.Request.Scheme}://{this.Request.Host}/{r.Icon}" : null,
            }).ToList();

            return this.CreatedAtAction(nameof(this.PostResources), null, response);
        }


        // DELETE: api/Resources
        [HttpDelete]
        public async Task<ActionResult> DeleteResources([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var resources = await _context.Resources.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (resources.Count == 0)
            {
                return this.NotFound("Nie znaleziono zasobów do usunięcia.");
            }

            this._context.Resources.RemoveRange(resources);
            await this._context.SaveChangesAsync();

            return this.Ok();
        }

        [HttpGet("nation/resource-balance/{nationId?}")]
        public async Task<ActionResult<NationResourceBalanceDto>> GetNationResourceBalance(int? nationId)
        {

            if (!nationId.HasValue)
            {
                nationId = this._nationId;
            }


            var resources = await this.GetAllResourcesAsync();
            var nation = await this.GetNationWithIncludesAsync((int)nationId);
            if (nation == null)
            {
                return this.NotFound();
            }

            var acceptedTradeAgreements = await this.GetAcceptedTradeAgreementsAsync((int)nationId);

            var result = new NationResourceBalanceDto
            {
                Resources = resources,
                ResourceBalances = new List<ResourceBalanceDto>(),
            };

            foreach (var resource in resources)
            {
                var resourceBalance = new ResourceBalanceDto
                {
                    ResourceId = resource.Id,
                    CurrentAmount = this.GetCurrentResourceAmount(nation, resource.Id),
                    ArmyMaintenanceExpenses = this.GetArmyMaintenanceExpenses(nation, resource.Id),
                    PopulationExpenses = this.GetPopulationExpenses(nation, resource.Id),
                    PopulationProduction = this.GetPopulationProduction(nation, resource.Id),
                    TradeIncome = this.GetTradeIncome(acceptedTradeAgreements, (int)nationId, resource.Id),
                    TradeExpenses = this.GetTradeExpenses(acceptedTradeAgreements, (int)nationId, resource.Id),
                    EventBalance = 0,
                };


                resourceBalance.TotalBalance =
                    resourceBalance.PopulationProduction
                    + resourceBalance.TradeIncome
                    - resourceBalance.ArmyMaintenanceExpenses
                    - resourceBalance.PopulationExpenses
                    - resourceBalance.TradeExpenses;


                ResourceChangeProcessor resourceChangeProcessor = new ResourceChangeProcessor(this._context, null);
                resourceBalance.EventBalance = await resourceChangeProcessor.CalculateChangeAsync((int)nationId, resourceBalance);
                resourceBalance.TotalBalance += resourceBalance.EventBalance;
                result.ResourceBalances.Add(resourceBalance);
            }

            return this.Ok(result);
        }

        // --- PODFUNKCJE ---

        private async Task<List<ResourceDto>> GetAllResourcesAsync()
        {
            return await this._context.Resources
                .AsNoTracking()
                .Select(r => new ResourceDto
                {
                    Id = r.Id.Value,
                    Name = r.Name,
                    IsMain = r.IsMain,
                })
                .ToListAsync();
        }

        private async Task<Nation?> GetNationWithIncludesAsync(int nationId)
        {
            return await _context.Nations
                .AsNoTracking()
                .Where(n => n.Id == nationId)
                .Include(n => n.Localisations)
                    .ThenInclude(l => l.LocalisationResources)
                .Include(n => n.Localisations)
                    .ThenInclude(l => l.Populations)
                        .ThenInclude(p => p.PopulationUsedResources)
                 .Include(n => n.Localisations)
                    .ThenInclude(l => l.Populations)
                        .ThenInclude(p => p.PopulationProductionShares)
                .Include(n => n.Armies)
                    .ThenInclude(a => a.Troops)
                        .ThenInclude(t => t.UnitType)
                            .ThenInclude(ut => ut.MaintenaceCosts)

                .FirstOrDefaultAsync();
        }

        private async Task<List<TradeAgreement>> GetAcceptedTradeAgreementsAsync(int nationId)
        {
            return await this._context.TradeAgreements
                .Where(ta => ta.Status == TradeStatus.Accepted && (ta.OfferingNationId == nationId || ta.ReceivingNationId == nationId))
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
                .SelectMany(p => p.PopulationUsedResources
                    .Where(pur => pur.ResourcesId == resourceId)
                    .Select(pur => pur.Amount))
                .Sum();
        }
        private float GetPopulationProduction(Nation nation, int resourceId)
        {
            return nation.Localisations
                .Sum(location => GetPopulationProductionByLocation(location, resourceId));
        }
        private float GetPopulationProductionByLocation(Localisation location, int resourceId)
        {
            if (location == null || location.LocalisationResources == null || !location.LocalisationResources.Any())
            {
                return 0;
            }

            float totalProduction = 0;

            foreach (var population in location.Populations)
            {

               var productionShare = population.PopulationProductionShares
                    .FirstOrDefault(ps => ps.ResourcesId == resourceId);

               var localisationResource = location.LocalisationResources
                    .FirstOrDefault(lr => lr.ResourceId == resourceId);

                if (localisationResource == null || productionShare == null)
                {
                    continue;
                }
                totalProduction += productionShare.Coefficient * localisationResource.Amount;
            }

            return totalProduction;
        }

        private float GetTradeIncome(List<TradeAgreement> tradeAgreements, int nationId, int resourceId)
        {
            float tradeIncome = 0;
            foreach (var ta in tradeAgreements)
            {
                bool isOffering = ta.OfferingNationId == nationId;
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
                bool isOffering = ta.OfferingNationId == nationId;
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

            if (nationId == null)
            {
                nationId = this._nationId;
            }

            var nation = await this._context.Nations
                .AsNoTracking()
                .Where(n => n.Id == nationId)
                .Include(n => n.Localisations)
                    .ThenInclude(l => l.LocalisationResources)
                        .ThenInclude(lr => lr.Resource)
                .FirstOrDefaultAsync();

            if (nation == null)
            {
                return this.NotFound($"Nie znaleziono państwa o ID: {nationId}");
            }

            var ownedResources = nation.Localisations
                .SelectMany(l => l.LocalisationResources)
                .GroupBy(lr => new { lr.ResourceId, lr.Resource.Name })
                .Select(g => new ResourceAmountDto
                {
                    ResourceId = g.Key.ResourceId,
                    ResourceName = g.Key.Name,
                    Amount = g.Sum(lr => lr.Amount),
                })
                .ToList();

            return this.Ok(ownedResources);
        }

        //private int GetEventBalance(Nation nation, int resourceId)
        //{
        //    return nation.RelatedEvents
        //        .SelectMany(re => re.Event.Modifiers
        //            .Where(m => m.ResourceId == resourceId && m.modifierType == ModifierType.Value)
        //            .Select(m => m.Amount))
        //        .Sum();
        //}



    }
}
