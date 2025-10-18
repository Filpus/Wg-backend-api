using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/ProductionCosts")]
    [ApiController]
    public class ProductionCostsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public ProductionCostsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;

            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }
            _context = _gameDbContextFactory.Create(schema);
        }

   

        // DELETE: api/ProductionCosts
        [HttpDelete]
        public async Task<ActionResult> DeleteProductionCosts([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var productionCosts = await _context.ProductionCosts.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (productionCosts.Count == 0)
            {
                return NotFound("Nie znaleziono kosztów produkcji do usunięcia.");
            }

            _context.ProductionCosts.RemoveRange(productionCosts);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("unitType/{unitTypeId}")]
        public async Task<ActionResult<List<UnitTypeResourceInfoDTO>>> GeProductionCostsForUnitType(int unitTypeId)
        {
            var list = await _context.ProductionCosts
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
        public async Task<ActionResult> UpsertProductionCosts([FromBody] List<UnitTypeResourceDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest("Brak danych do przetworzenia.");
            }

            foreach (var dto in dtos)
            {


                ProductionCost entity = null;

                if (dto.Id.HasValue)
                {
                    entity = await _context.ProductionCosts.FindAsync(dto.Id.Value);
                }

                if (entity == null)
                {
                    entity = new ProductionCost
                    {
                        UnitTypeId = dto.UnitTypeId,
                        ResourceId = dto.ResourceId,
                        Amount = dto.Amount
                    };
                    await _context.ProductionCosts.AddAsync(entity);
                }
                else
                {
                    entity.UnitTypeId = dto.UnitTypeId;
                    entity.ResourceId = dto.ResourceId;
                    entity.Amount = dto.Amount;
                    _context.ProductionCosts.Update(entity);
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
