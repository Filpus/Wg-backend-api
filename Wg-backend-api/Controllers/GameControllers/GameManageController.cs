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
    [AuthorizeGameRole("GameMaster", "Player")]
    public class GameManageController : GameControllerBase
    {
        private readonly ModifierProcessorFactory _processorFactory;

        public GameManageController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, ModifierProcessorFactory modifierProcessorFactory)
            : base(gameDbFactory, sessionDataService)
        {
            this._processorFactory = modifierProcessorFactory;
        }

        [HttpPost("EndTurn")]
        public async Task<IActionResult> EndTurn()
        {
            try
            {
                await this.ResolveResourceBalance();
                await this.ResolveArmyRecrutment();
                await this.ResolveTradeAgreements();
                await this.Context.SaveChangesAsync();

                return Ok(new { message = "Tura zakończona pomyślnie." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Wystąpił błąd podczas kończenia tury.", error = ex.Message });
            }
        }

        private async Task ResolveResourceBalance()
        {
            var nations = await this.Context.Nations.ToListAsync();

            foreach (var nation in nations)
            {
                var ownedResources = await this.Context.Set<OwnedResources>()
                    .Where(or => or.NationId == nation.Id)
                    .ToListAsync();

                var nationBalance = await CalcResourceBalance.CalculateNationResourceBalance((int)nation.Id, this.Context);

                foreach (var resourceBalance in nationBalance.ResourceBalances)
                {
                    if (resourceBalance.TotalBalance == 0)
                    {
                        continue;
                    }

                    var ownedResource = ownedResources.FirstOrDefault(or => or.ResourceId == resourceBalance.ResourceId);
                    if (ownedResource != null)
                    {
                        ownedResource.Amount += resourceBalance.TotalBalance;
                    }
                    else
                    {

                        this.Context.Set<OwnedResources>().Add(new OwnedResources
                        {
                            NationId = (int)nation.Id,
                            ResourceId = resourceBalance.ResourceId,
                            Amount = resourceBalance.TotalBalance,
                        });
                    }
                }
            }
        }

        private async Task ResolveTradeAgreements()
        {
            var tradeAgreements = await this.Context.TradeAgreements
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

        }

        private async Task ResolveArmyRecrutment()
        {
            const string landBarracksName = "Baraki";
            const string navalBarracksName = "Doki";

            var nations = await this.Context.Nations
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
                    var unitType = await this.Context.Set<UnitType>().FindAsync(order.UnitTypeId);
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
                            this.Context.Armies.Add(navalBarracks);
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
                            this.Context.Armies.Add(landBarracks);
                            nation.Armies.Add(landBarracks);
                        }

                        targetArmy = landBarracks;
                    }


                    for (int i = 0; i < order.Quantity; i++)
                    {
                        var troop = new Troop
                        {
                            UnitTypeId = order.UnitTypeId,
                            Army = targetArmy,
                            Quantity = unitType.VolunteersNeeded,
                        };

                        targetArmy.Troops.Add(troop);
                        this.Context.Troops.Add(troop);
                    }

                    this.Context.UnitOrders.Remove(order);
                }
            }
        }

    }
}
