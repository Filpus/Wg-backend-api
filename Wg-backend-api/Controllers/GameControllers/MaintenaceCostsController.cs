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
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class MaintenaceCostsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public MaintenaceCostsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = this._gameDbContextFactory.Create(schema);
        }

        [HttpGet("unitType/{unitTypeId}")]
        public async Task<ActionResult<List<UnitTypeResourceInfoDTO>>> GetMaintenaceCostsForUnitType(int unitTypeId)
        {
            var list = await this._context.MaintenaceCosts
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
                    entity = await this._context.MaintenaceCosts.FindAsync(dto.Id.Value);
                }

                if (entity == null)
                {
                    entity = new MaintenaceCosts
                    {
                        UnitTypeId = dto.UnitTypeId,
                        ResourceId = dto.ResourceId,
                        Amount = dto.Amount
                    };
                    await this._context.MaintenaceCosts.AddAsync(entity);
                }
                else
                {
                    entity.UnitTypeId = dto.UnitTypeId;
                    entity.ResourceId = dto.ResourceId;
                    entity.Amount = dto.Amount;
                    this._context.MaintenaceCosts.Update(entity);
                }
            }

            await this._context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteMaintenaceCosts([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var maintenaceCosts = await this._context.MaintenaceCosts.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (maintenaceCosts.Count == 0)
            {
                return NotFound("Nie znaleziono kosztów utrzymania do usunięcia.");
            }

            this._context.MaintenaceCosts.RemoveRange(maintenaceCosts);
            await this._context.SaveChangesAsync();

            return Ok();
        }
    }
}
