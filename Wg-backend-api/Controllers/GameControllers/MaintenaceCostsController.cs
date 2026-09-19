using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/MaintenaceCosts")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class MaintenaceCostsController : GameControllerBase
    {
        public MaintenaceCostsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        [HttpGet("unitType/{unitTypeId}")]
        public async Task<ActionResult<List<UnitTypeResourceInfoDTO>>> GetMaintenaceCostsForUnitType(int unitTypeId)
        {
            var list = await this.Context.MaintenaceCosts
                .Where(m => m.UnitTypeId == unitTypeId)
                .Include(m => m.UnitType)
                .Include(m => m.Resource)
                .Select(m => new UnitTypeResourceInfoDTO
                {
                    Id = (int)m.Id,
                    UnitTypeId = m.UnitTypeId,
                    UnitTypeName = m.UnitType.Name,
                    ResourceId = m.ResourceId,
                    ResourceName = m.Resource.Name,
                    Amount = m.Amount
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult> UpsertMaintenaceCosts([FromBody] List<UnitTypeResourceDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest("Brak danych do przetworzenia.");
            }

            foreach (var dto in dtos)
            {

                MaintenaceCosts entity = null;

                if (dto.Id.HasValue)
                {
                    entity = await this.Context.MaintenaceCosts.FindAsync(dto.Id.Value);
                }

                if (entity == null)
                {
                    entity = new MaintenaceCosts
                    {
                        UnitTypeId = dto.UnitTypeId,
                        ResourceId = dto.ResourceId,
                        Amount = dto.Amount
                    };
                    await this.Context.MaintenaceCosts.AddAsync(entity);
                }
                else
                {
                    entity.UnitTypeId = dto.UnitTypeId;
                    entity.ResourceId = dto.ResourceId;
                    entity.Amount = dto.Amount;
                    this.Context.MaintenaceCosts.Update(entity);
                }
            }

            await this.Context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteMaintenaceCosts([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var maintenaceCosts = await this.Context.MaintenaceCosts.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (maintenaceCosts.Count == 0)
            {
                return NotFound("Nie znaleziono kosztów utrzymania do usunięcia.");
            }

            this.Context.MaintenaceCosts.RemoveRange(maintenaceCosts);
            await this.Context.SaveChangesAsync();

            return Ok();
        }
    }
}
