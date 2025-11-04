using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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
    public class GameManageController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private readonly ModifierProcessorFactory _processorFactory;
        private readonly GlobalDbContext _globalDbContext;
        private readonly string _schema;
        private readonly int? _nationId;

        public GameManageController(
            IGameDbContextFactory gameDbFactory,
            ISessionDataService sessionDataService,
            ModifierProcessorFactory modifierProcessorFactory,
            GlobalDbContext globalDbContext)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;
            _processorFactory = modifierProcessorFactory;
            _globalDbContext = globalDbContext;

            _schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(_schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            _nationId = _sessionDataService.GetNation() != null ?
                int.Parse(_sessionDataService.GetNation()) : null;
        }

        [HttpPost("EndTurn")]
        public async Task<IActionResult> EndTurn()
        {
            try
            {
                using var context = _gameDbContextFactory.Create(_schema);

                await ResolveResourceBalance(context);
                await ResolveArmyRecrutment(context);
                await ResolveTradeAgreements(context);

                var gameId = int.Parse(_schema.Split('_')[1]);
                var game = await _globalDbContext.Games.FirstOrDefaultAsync(g => g.Id == gameId);
                if (game != null)
                {
                    game.Turn += 1;
                    game.TurnStartDate = DateTime.UtcNow;
                    await _globalDbContext.SaveChangesAsync();
                }
               

                return Ok(new { message = "Tura zakończona pomyślnie." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Wystąpił błąd podczas kończenia tury.",
                    error = ex.Message
                });
            }
        }

        // Przekaż context jako parametr zamiast używać pola
        private async Task ResolveResourceBalance(GameDbContext context)
        {
            var nations = await context.Nations.ToListAsync();

            foreach (var nation in nations)
            {
                var ownedResources = await context.Set<OwnedResources>()
                    .Where(or => or.NationId == nation.Id)
                    .ToListAsync();

                var nationBalance = await CalcResourceBalance
                    .CalculateNationResourceBalance((int)nation.Id, context);

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

            await context.SaveChangesAsync();
        }

        private async Task ResolveTradeAgreements(GameDbContext context)
        {
            var tradeAgreements = await context.TradeAgreements
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

            await context.SaveChangesAsync();
        }

        private async Task ResolveArmyRecrutment(GameDbContext context)
        {
            var nations = await context.Nations
                .Include(n => n.Armies)
                .Include(n => n.UnitOrders)
                .Include(n => n.Localisations) 
                .AsSplitQuery() 
                .ToListAsync();

            foreach (var nation in nations)
            {
                var recruitOrders = nation.UnitOrders.ToList();
                if (!recruitOrders.Any())
                    continue;

                var recruitsArmy = nation.Armies.FirstOrDefault(a => a.Name == "Rekruci");
                if (recruitsArmy == null)
                {
                    var randomLocation = nation.Localisations.FirstOrDefault().Id;
                    if (randomLocation == null)
                        throw new InvalidOperationException($"Brak lokalizacji powiązanych z państwem o ID {nation.Id}.");

                    recruitsArmy = new Army
                    {
                        NationId = (int)nation.Id,
                        LocationId = (int)nation.Localisations.FirstOrDefault().Id,
                        Name = "Rekruci",
                        Troops = new List<Troop>()
                    };
                    var addedArmy = context.Armies.Add(recruitsArmy);
                    await context.SaveChangesAsync();
                    recruitsArmy.Id = addedArmy.Entity.Id;

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
                        context.Troops.Add(troop);
                    }
                    context.UnitOrders.Remove(order);
                }
            }

            await context.SaveChangesAsync();
        }
    }

}
