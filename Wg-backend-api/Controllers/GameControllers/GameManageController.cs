using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers;
using Wg_backend_api.Logic.Resources;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/GameManage")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class GameManageController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private readonly ModifierProcessorFactory _processorFactory;

        private GameDbContext _context;
        private int? _nationId;

        public GameManageController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, ModifierProcessorFactory modifierProcessorFactory)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;
            this._processorFactory = modifierProcessorFactory;
            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = this._gameDbContextFactory.Create(schema);
            this._nationId = this._sessionDataService.GetNation() != null ? int.Parse(this._sessionDataService.GetNation()) : null;
        }

        [HttpPost("EndTurn")]
        public async Task<IActionResult> EndTurn()
        {

            try
            {
                await this.ResolveResourceBalance();
                await this.ResolveArmyRecrutment();
                await this.ResolveTradeAgreements();

                return Ok(new { message = "Tura zakończona pomyślnie." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Wystąpił błąd podczas kończenia tury.", error = ex.Message });
            }
        }

        private async Task ResolveResourceBalance()
        {
            var nations = await this._context.Nations.ToListAsync();

            foreach (var nation in nations)
            {
                var ownedResources = await this._context.Set<OwnedResources>()
                    .Where(or => or.NationId == nation.Id)
                    .ToListAsync();

                var nationBalance = await CalcResourceBalance.CalculateNationResourceBalance((int)nation.Id, this._context);

                foreach (var ownedResource in ownedResources)
                {
                    var resourceBalance = nationBalance.ResourceBalances
                        .FirstOrDefault(rb => rb.ResourceId == ownedResource.ResourceId);

                    if (resourceBalance != null)
                    {
                        ownedResource.Amount += resourceBalance.TotalBalance;
                    }
                }
            }

            await this._context.SaveChangesAsync();
        }

        private async Task ResolveTradeAgreements()
        {
            var tradeAgreements = await this._context.TradeAgreements
                .Where(ta => ta.Status == TradeStatus.Accepted && ta.Duration > 0)
                .ToListAsync();

            foreach (var agreement in tradeAgreements)
            {
                agreement.Duration -= 1;
                if (agreement.Duration == 0)
                {
                    agreement.Status = TradeStatus.Ended;
                }
            }

            await this._context.SaveChangesAsync();
        }

        private async Task ResolveArmyRecrutment()
        {
            const string landBarracksName = "Baraki";
            const string navalBarracksName = "Doki";

            var nations = await this._context.Nations
                .Include(n => n.Armies)
                    .ThenInclude(a => a.Troops)
                .Include(n => n.UnitOrders)
                .ToListAsync();

            foreach (var nation in nations)
            {
                var recruitOrders = nation.UnitOrders.ToList();
                if (!recruitOrders.Any())
                {
                    continue;
                }

                var landBarracks = nation.Armies.FirstOrDefault(a => a.LocationId == null && !a.IsNaval);
                var navalBarracks = nation.Armies.FirstOrDefault(a => a.LocationId == null && a.IsNaval);

                foreach (var order in recruitOrders)
                {
                    var unitType = await this._context.Set<UnitType>().FindAsync(order.UnitTypeId);
                    bool isNaval = unitType != null && unitType.IsNaval;

                    Army targetArmy;
                    if (isNaval)
                    {
                        if (navalBarracks == null)
                        {
                            navalBarracks = new Army
                            {
                                NationId = (int)nation.Id,
                                Name = navalBarracksName,
                                IsNaval = true,
                                Troops = []
                            };
                            this._context.Armies.Add(navalBarracks);
                            nation.Armies.Add(navalBarracks);
                        }

                        targetArmy = navalBarracks;
                    }
                    else
                    {
                        if (landBarracks == null)
                        {
                            landBarracks = new Army
                            {
                                NationId = (int)nation.Id,
                                Name = landBarracksName,
                                IsNaval = false,
                                Troops = []
                            };
                            this._context.Armies.Add(landBarracks);
                            nation.Armies.Add(landBarracks);
                        }

                        targetArmy = landBarracks;
                    }

                    int amount = order.Quantity;
                    for (int i = 0; i < amount; i++)
                    {
                        var troop = new Troop
                        {
                            UnitTypeId = order.UnitTypeId,
                            Army = targetArmy
                        };

                        targetArmy.Troops.Add(troop);
                        this._context.Troops.Add(troop);
                    }

                    this._context.UnitOrders.Remove(order);
                }
            }

            await this._context.SaveChangesAsync();
        }

    }
}
