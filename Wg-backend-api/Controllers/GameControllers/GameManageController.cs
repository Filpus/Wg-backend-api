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
                ResolveResourceBalance();
                ResolveArmyRecrutment();
                ResolveTradeAgreements();

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
            var nations = await this._context.Nations
                .Include(n => n.Armies)
                .Include(n => n.UnitOrders)
                .ToListAsync();

            foreach (var nation in nations)
            {
                var recruitOrders = nation.UnitOrders.ToList();
                if (!recruitOrders.Any())
                {
                    continue;
                }

                var recruitsArmy = nation.Armies.FirstOrDefault(a => a.Name == "Rekruci");
                if (recruitsArmy == null)
                {
                    recruitsArmy = new Army
                    {
                        NationId = (int)nation.Id,
                        Name = "Rekruci",
                        Troops = []
                    };
                    this._context.Armies.Add(recruitsArmy);
                    nation.Armies.Add(recruitsArmy);
                }

                foreach (var order in recruitOrders)
                {
                    int amount = order.Quantity;
                    for (int i = 0; i < amount; i++)
                    {
                        var troop = new Troop
                        {
                            UnitTypeId = order.UnitTypeId,
                            Army = recruitsArmy,
                            ArmyId = (int)recruitsArmy.Id
                        };
                        recruitsArmy.Troops.Add(troop);
                        this._context.Troops.Add(troop);
                    }

                    this._context.UnitOrders.Remove(order);
                }
            }

            await this._context.SaveChangesAsync();
        }

    }
}
