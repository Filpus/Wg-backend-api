using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Helpers;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/UnitOrders")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class UnitOrdersController : GameControllerBase
    {
        public UnitOrdersController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        // GET: api/UnitOrders
        // GET: api/UnitOrders/5

        // DELETE: api/UnitOrders
        [HttpDelete]
        public async Task<ActionResult> DeleteUnitOrders([FromBody] List<int?> ids)
        {
            var missingIds = this.ValidateIdsProvided(ids);
            if (missingIds != null)
            {
                return missingIds;
            }

            var unitOrders = await this.Context.UnitOrders.Where(r => ids.Contains(r.Id)).ToListAsync();

            var noneFound = this.NotFoundIfNoneFound(unitOrders, "zamówień jednostek");
            if (noneFound != null)
            {
                return noneFound;
            }

            this.Context.UnitOrders.RemoveRange(unitOrders);
            await this.Context.SaveChangesAsync();

            return this.Ok();
        }

        [HttpGet("GetUnitOrdersByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitOrderInfoDTO>>> GetNavUnitOrdersByNationId(int? nationId)
        {
            nationId ??= this.NationId;

            var unitOrders = await this.Context.UnitOrders
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
            nationId ??= this.NationId;

            var navalUnitOrders = await this.Context.UnitOrders
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
            nationId ??= this.NationId;

            var landUnitOrders = await this.Context.UnitOrders
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
            nationId ??= this.NationId;

            if (recruitOrder == null || recruitOrder.Count <= 0)
            {
                return this.BadRequest("Nieprawidłowe dane zlecenia rekrutacji.");
            }

            var nationExists = await this.Context.Nations.AnyAsync(n => n.Id == nationId);
            if (!nationExists)
            {
                return this.NotFound("Nie znaleziono państwa o podanym ID.");
            }

            var unitTypeExists = await this.Context.UnitTypes.AnyAsync(ut => ut.Id == recruitOrder.TroopTypeId);
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

            this.Context.UnitOrders.Add(newUnitOrder);
            await this.Context.SaveChangesAsync();

            return this.Ok(newUnitOrder);
        }

        [HttpPatch("UpdateRecruitmentCount")]
        public async Task<IActionResult> UpdateRecruitmentCount([FromBody] EditOrderDTO editOrder)
        {
            if (editOrder == null || editOrder.OrderId <= 0 || editOrder.NewCount < 0)
            {
                return this.BadRequest("Nieprawidłowe dane zlecenia edycji.");
            }

            var unitOrder = await this.Context.UnitOrders.FindAsync(editOrder.OrderId);
            if (unitOrder == null)
            {
                return this.NotFound("Nie znaleziono zamówienia jednostek o podanym ID.");
            }

            unitOrder.Quantity = editOrder.NewCount;

            this.Context.Entry(unitOrder).State = EntityState.Modified;

            try
            {
                await this.Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return this.StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return this.Ok(unitOrder);
        }
    }
}
