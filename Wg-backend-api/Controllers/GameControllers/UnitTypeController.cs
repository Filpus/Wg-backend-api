using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/UnitTypes")]
    [ApiController]
    public class UnitTypeController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public UnitTypeController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        // GET: api/UnitTypes
        // GET: api/UnitTypes/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<UnitType>>> GetUnitTypes(int? id)
        {
            if (id.HasValue)
            {
                var unitType = await _context.UnitTypes.FindAsync(id);
                if (unitType == null)
                {
                    return NotFound();
                }
                return Ok(new List<UnitType> { unitType });
            }
            else
            {
                return await _context.UnitTypes.ToListAsync();
            }
        }

        // PUT: api/UnitTypes
        [HttpPut]
        public async Task<IActionResult> PutUnitTypes([FromBody] List<UnitType> unitTypes)
        {
            if (unitTypes == null || unitTypes.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var unitType in unitTypes)
            {
                _context.Entry(unitType).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        // POST: api/UnitTypes
        [HttpPost]
        public async Task<ActionResult<UnitType>> PostUnitTypes([FromBody] List<UnitType> unitTypes)
        {
            if (unitTypes == null || unitTypes.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (UnitType unitType in unitTypes)
            {
                unitType.Id = null;
            }

            _context.UnitTypes.AddRange(unitTypes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnitTypes", new { id = unitTypes[0].Id }, unitTypes);
        }

        // DELETE: api/UnitTypes
        [HttpDelete]
        public async Task<ActionResult> DeleteUnitTypes([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var unitTypes = await _context.UnitTypes.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (unitTypes.Count == 0)
            {
                return NotFound("Nie znaleziono jednostek do usunięcia.");
            }

            _context.UnitTypes.RemoveRange(unitTypes);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("GetLandUnitTypeInfo/{nationId}")]
        public async Task<ActionResult<IEnumerable<UnitTypeInfoDTO>>> GetLandUnitTypeInfo(int nationId)
        {
            var accessibleUnitTypeIds = await _context.AccessToUnits
                .Where(atu => atu.NationId == nationId)
                .Select(atu => atu.UnitTypeId)
                .ToListAsync();

            var unitTypes = await _context.UnitTypes
                .Where(ut => !ut.IsNaval && accessibleUnitTypeIds.Contains(ut.Id.Value))
                .Include(ut => ut.ProductionCosts)
                .Include(ut => ut.MaintenaceCosts)
                .ToListAsync();

            var unitTypeInfoList = unitTypes.Select(ut => new UnitTypeInfoDTO
            {
                UnitId = ut.Id.Value,
                UnitTypeName = ut.Name,
                Description = ut.Description,
                Quantity = ut.VolunteersNeeded,
                Melee = ut.Melee,
                Range = ut.Range,
                Defense = ut.Defense,
                Speed = ut.Speed,
                Morale = ut.Morale,
                IsNaval = ut.IsNaval,
                ConsumedResources = GetConsumedResources(ut),
                ProductionCost = GetProductionCost(ut)
            }).ToList();

            return Ok(unitTypeInfoList);
        }
        [HttpGet("GetNavalUnitTypeInfo/{nationId}")]
        public async Task<ActionResult<IEnumerable<UnitTypeInfoDTO>>> GetNavalUnitTypeInfo(int nationId)
        {
            var accessibleUnitTypeIds = await _context.AccessToUnits
                .Where(atu => atu.NationId == nationId)
                .Select(atu => atu.UnitTypeId)
                .ToListAsync();

            var unitTypes = await _context.UnitTypes
                .Where(ut => ut.IsNaval && accessibleUnitTypeIds.Contains(ut.Id.Value))
                .Include(ut => ut.ProductionCosts)
                .Include(ut => ut.MaintenaceCosts)
                .ToListAsync();

            var unitTypeInfoList = unitTypes.Select(ut => new UnitTypeInfoDTO
            {
                UnitId = ut.Id.Value,
                UnitTypeName = ut.Name,
                Description = ut.Description,
                Quantity = ut.VolunteersNeeded,
                Melee = ut.Melee,
                Range = ut.Range,
                Defense = ut.Defense,
                Speed = ut.Speed,
                Morale = ut.Morale,
                IsNaval = ut.IsNaval,
                ConsumedResources = GetConsumedResources(ut),
                ProductionCost = GetProductionCost(ut)
            }).ToList();

            return Ok(unitTypeInfoList);
        }
        private List<ResourceAmountDto> GetConsumedResources(UnitType unitType)
        {
            return unitType.MaintenaceCosts.Select(mc => new ResourceAmountDto
            {
                ResourceId = mc.ResourceId,
                ResourceName = mc.Resource.Name,
                Amount = mc.Amount
            }).ToList();
        }

        private List<ResourceAmountDto> GetProductionCost(UnitType unitType)
        {
            return unitType.ProductionCosts.Select(pc => new ResourceAmountDto
            {
                ResourceId = pc.ResourceId,
                ResourceName = pc.Resource.Name,
                Amount = pc.Amount
            }).ToList();
        }
    }
}
