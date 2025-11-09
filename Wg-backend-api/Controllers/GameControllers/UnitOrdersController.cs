namespace Wg_backend_api.Controllers.GameControllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Wg_backend_api.Auth;
    using Wg_backend_api.Data;
    using Wg_backend_api.DTO;
    using Wg_backend_api.Models;
    using Wg_backend_api.Services;

    [Route("api/UnitOrders")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class UnitOrdersController : Controller
    {
        private readonly IGameDbContextFactory gameDbContextFactory;
        private readonly ISessionDataService sessionDataService;
        private readonly GameDbContext context;
        private readonly int? nationId;

        public UnitOrdersController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this.gameDbContextFactory = gameDbFactory;
            this.sessionDataService = sessionDataService;

            string schema = this.sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this.context = this.gameDbContextFactory.Create(schema);
            string nationIdStr = this.sessionDataService.GetNation();
            this.nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
        }

        // GET: api/UnitOrders
        // GET: api/UnitOrders/5

        // DELETE: api/UnitOrders
        [HttpDelete]
        public async Task<ActionResult> DeleteUnitOrders([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var unitOrders = await this.context.UnitOrders.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (unitOrders.Count == 0)
            {
                return this.NotFound("Nie znaleziono zamówień jednostek do usunięcia.");
            }

            this.context.UnitOrders.RemoveRange(unitOrders);
            await this.context.SaveChangesAsync();

            return this.Ok();
        }

        [HttpGet("GetUnitOrdersByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitOrderInfoDTO>>> GetNavUnitOrdersByNationId(int? nationId)
        {
            if (nationId == null)
            {
                nationId = this.nationId;
            }

            var unitOrders = await this.context.UnitOrders
                .Where(uo => uo.NationId == nationId)
                .Select(uo => new UnitOrderInfoDTO
                {
                    Id = uo.Id,
                    UnitTypeName = uo.UnitType.Name,
                    UnitTypeId = uo.UnitTypeId,

                    Quantity = uo.Quantity,
                    UsedManpower = uo.Quantity * uo.UnitType.VolunteersNeeded, // Calculate used manpower
                })
                .ToListAsync();

            return this.Ok(unitOrders);
        }

        [HttpGet("GetNavalUnitOrdersByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitOrderInfoDTO>>> GetNavalUnitOrdersByNationId(int? nationId)
        {
            if (nationId == null)
            {
                nationId = this.nationId;
            }

            var navalUnitOrders = await this.context.UnitOrders
                .Where(uo => uo.NationId == nationId && uo.UnitType.IsNaval) // Assuming UnitType has an IsNaval property
                .Select(uo => new UnitOrderInfoDTO
                {
                    Id = uo.Id,
                    UnitTypeName = uo.UnitType.Name,
                    Quantity = uo.Quantity,
                    UnitTypeId = uo.UnitTypeId,

                    UsedManpower = uo.Quantity * uo.UnitType.VolunteersNeeded, // Calculate used manpower
                })
                .ToListAsync();

            return this.Ok(navalUnitOrders);
        }

        [HttpGet("GetLandUnitOrdersByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitOrderInfoDTO>>> GetLandUnitOrdersByNationId(int? nationId)
        {
            if (nationId == null)
            {
                nationId = this.nationId;
            }

            var landUnitOrders = await this.context.UnitOrders
                .Where(uo => uo.NationId == nationId && !uo.UnitType.IsNaval) // Assuming UnitType has an IsNaval property
                .Select(uo => new UnitOrderInfoDTO
                {
                    Id = uo.Id,
                    UnitTypeName = uo.UnitType.Name,
                    UnitTypeId = uo.UnitTypeId,
                    Quantity = uo.Quantity,
                    UsedManpower = uo.Quantity * uo.UnitType.VolunteersNeeded, // Calculate used manpower
                })
                .ToListAsync();

            return this.Ok(landUnitOrders);
        }

        [HttpPost("AddRecruitOrder/{nationId?}")]
        public async Task<IActionResult> AddRecruitOrder(int? nationId, [FromBody] RecruitOrderDTO recruitOrder)
        {
            if (nationId == null)
            {
                nationId = this.nationId;
            }

            if (recruitOrder == null || recruitOrder.Count <= 0)
            {
                return this.BadRequest("Nieprawidłowe dane zlecenia rekrutacji.");
            }

            var nationExists = await this.context.Nations.AnyAsync(n => n.Id == nationId);
            if (!nationExists)
            {
                return this.NotFound("Nie znaleziono państwa o podanym ID.");
            }

            var unitTypeExists = await this.context.UnitTypes.AnyAsync(ut => ut.Id == recruitOrder.TroopTypeId);
            if (!unitTypeExists)
            {
                return this.NotFound("Nie znaleziono typu jednostki o podanym ID.");
            }

            var newUnitOrder = new UnitOrder
            {
                NationId = (int)nationId,
                UnitTypeId = recruitOrder.TroopTypeId,
                Quantity = recruitOrder.Count,
            };

            this.context.UnitOrders.Add(newUnitOrder);
            await this.context.SaveChangesAsync();

            return this.Ok(newUnitOrder);
        }

        [HttpPatch("UpdateRecruitmentCount")]
        public async Task<IActionResult> UpdateRecruitmentCount([FromBody] EditOrderDTO editOrder)
        {
            if (editOrder == null || editOrder.OrderId <= 0 || editOrder.NewCount < 0)
            {
                return this.BadRequest("Nieprawidłowe dane zlecenia edycji.");
            }

            var unitOrder = await this.context.UnitOrders.FindAsync(editOrder.OrderId);
            if (unitOrder == null)
            {
                return this.NotFound("Nie znaleziono zamówienia jednostek o podanym ID.");
            }

            unitOrder.Quantity = editOrder.NewCount;

            this.context.Entry(unitOrder).State = EntityState.Modified;

            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return this.StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return this.Ok(unitOrder);
        }
    }
}
