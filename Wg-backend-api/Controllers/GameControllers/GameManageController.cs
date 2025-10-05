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

        private GameDbContext _context;
        private int? _nationId;


        public GameManageController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, ModifierProcessorFactory modifierProcessorFactory)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;
            _processorFactory = modifierProcessorFactory;
            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }
            _context = _gameDbContextFactory.Create(schema);
            _nationId = _sessionDataService.GetNation() != null ? int.Parse(_sessionDataService.GetNation()) : null;
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
            var nations = await _context.Nations.ToListAsync();

            foreach (var nation in nations)
            {
                var ownedResources = await _context.Set<OwnedResouerce>()
                    .Where(or => or.NationId == nation.Id)
                    .ToListAsync();

                var nationBalance = await CalcResourceBalance.CalculateNationResourceBalance((int)nation.Id, _context);

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

            await _context.SaveChangesAsync();
        }

        private async Task ResolveTradeAgreements()
        {
            var tradeAgreements = await _context.TradeAgreements
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

            await _context.SaveChangesAsync();
        }

        private async Task ResolveArmyRecrutment()
        {
            var nations = await _context.Nations
                .Include(n => n.Armies)
                .Include(n => n.UnitOrders)
                .ToListAsync();

            foreach (var nation in nations)
            {
                var recruitOrders = nation.UnitOrders.ToList();
                if (!recruitOrders.Any())
                    continue;

                var recruitsArmy = nation.Armies.FirstOrDefault(a => a.Name == "Rekruci");
                if (recruitsArmy == null)
                {
                    recruitsArmy = new Army
                    {
                        NationId = (int)nation.Id,
                        Name = "Rekruci",
                        Troops = new List<Troop>()
                    };
                    _context.Armies.Add(recruitsArmy);
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
                        _context.Troops.Add(troop);
                    }
                    _context.UnitOrders.Remove(order);
                }
            }

            await _context.SaveChangesAsync();
        }

        
    }
}
