using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Logic.Modifiers.Processors;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Resources
{
    public static class CalcResourceBalance
    {


        public static async Task<NationResourceBalanceDto> CalculateNationResourceBalance(int nationId, GameDbContext _context)
        {
            var nation = await GetNationWithIncludesAsync(nationId, _context);
            if (nation == null)
                return null;

            var resources = await GetAllResourcesAsync(_context);
            var tradeAgreements = await GetAcceptedTradeAgreementsAsync(nationId, _context);

            var result = new NationResourceBalanceDto
            {
                Resources = resources,
                ResourceBalances = new List<ResourceBalanceDto>()
            };

            var processor = new ResourceChangeProcessor(_context, null);

            foreach (var resource in resources)
            {
                var balance = new ResourceBalanceDto
                {
                    ResourceId = resource.Id,
                    CurrentAmount = GetCurrentResourceAmount(nation, resource.Id),
                    ArmyMaintenanceExpenses = GetArmyMaintenanceExpenses(nation, resource.Id),
                    PopulationExpenses = GetPopulationExpenses(nation, resource.Id),
                    PopulationProduction = GetPopulationProduction(nation, resource.Id),
                    TradeIncome = GetTradeIncome(tradeAgreements, nationId, resource.Id),
                    TradeExpenses = GetTradeExpenses(tradeAgreements, nationId, resource.Id)
                };

                balance.TotalBalance =
                    balance.PopulationProduction
                    + balance.TradeIncome
                    - balance.ArmyMaintenanceExpenses
                    - balance.PopulationExpenses
                    - balance.TradeExpenses;

                balance.EventBalance = await processor.CalculateChangeAsync(nationId, balance);
                balance.TotalBalance += balance.EventBalance;

                result.ResourceBalances.Add(balance);
            }

            return result;
        }

        public static async Task<List<ResourceDto>> GetAllResourcesAsync(GameDbContext _context)
        {
            return await _context.Resources
                .AsNoTracking()
                .Select(r => new ResourceDto
                {
                    Id = r.Id.Value,
                    Name = r.Name,
                    IsMain = r.IsMain,
                })
                .ToListAsync();
        }

        public static async Task<Nation?> GetNationWithIncludesAsync(int nationId, GameDbContext _context)
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

        public static async Task<List<TradeAgreement>> GetAcceptedTradeAgreementsAsync(int nationId, GameDbContext _context)
        {
            return await _context.TradeAgreements
                .Where(ta => ta.Status == TradeStatus.Accepted && (ta.OfferingNationId == nationId || ta.ReceivingNationId == nationId))
                .Include(ta => ta.OfferedResources)
                .Include(ta => ta.WantedResources)
                .ToListAsync();
        }

        public static float GetCurrentResourceAmount(Nation nation, int resourceId)
        {
            return nation.Localisations
                .SelectMany(l => l.LocalisationResources)
                .Where(lr => lr.ResourceId == resourceId)
                .Sum(lr => lr.Amount);
        }

        public static float GetArmyMaintenanceExpenses(Nation nation, int resourceId)
        {
            return nation.Armies
                .SelectMany(a => a.Troops)
                .SelectMany(t => t.UnitType.MaintenaceCosts
                    .Where(mc => mc.ResourceId == resourceId)
                    .Select(mc => mc.Amount * t.Quantity))
                .Sum();
        }

        public static float GetPopulationExpenses(Nation nation, int resourceId)
        {
            return nation.Localisations
                .SelectMany(l => l.Populations)
                .SelectMany(p => p.PopulationUsedResources
                    .Where(pur => pur.ResourcesId == resourceId)
                    .Select(pur => pur.Amount))
                .Sum();
        }
        public static float GetPopulationProduction(Nation nation, int resourceId)
        {
            return nation.Localisations
                .Sum(location => GetPopulationProductionByLocation(location, resourceId));
        }
        public static float GetPopulationProductionByLocation(Localisation location, int resourceId)
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

        public static float GetTradeIncome(List<TradeAgreement> tradeAgreements, int nationId, int resourceId)
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

        public static float GetTradeExpenses(List<TradeAgreement> tradeAgreements, int nationId, int resourceId)
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
    }
}
