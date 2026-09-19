using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/UnitTypes")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class UnitTypeController : GameControllerBase
    {
        public UnitTypeController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        // GET: api/UnitTypes  
        // GET: api/UnitTypes/5  
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<UnitTypeDTO>>> GetUnitTypes(int? id)
        {
            if (id.HasValue)
            {
                var unitType = await this.Context.UnitTypes.FindAsync(id);
                if (unitType == null)
                {
                    return NotFound();
                }

                var unitTypeDTO = new UnitTypeDTO
                {
                    UnitId = unitType.Id.Value,
                    UnitName = unitType.Name,
                    Description = unitType.Description,
                    Quantity = unitType.VolunteersNeeded,
                    Melee = unitType.Melee,
                    Range = unitType.Range,
                    Defense = unitType.Defense,
                    Speed = unitType.Speed,
                    Morale = unitType.Morale,
                    IsNaval = unitType.IsNaval
                };
                return Ok(new List<UnitTypeDTO> { unitTypeDTO });
            }
            else
            {
                var unitTypes = await this.Context.UnitTypes.ToListAsync();
                var unitTypeDTOs = unitTypes.Select(ut => new UnitTypeDTO
                {
                    UnitId = ut.Id.Value,
                    UnitName = ut.Name,
                    Description = ut.Description,
                    Quantity = ut.VolunteersNeeded,
                    Melee = ut.Melee,
                    Range = ut.Range,
                    Defense = ut.Defense,
                    Speed = ut.Speed,
                    Morale = ut.Morale,
                    IsNaval = ut.IsNaval
                }).ToList();
                return Ok(unitTypeDTOs);
            }
        }

        // PUT: api/UnitTypes  
        [HttpPut]
        public async Task<IActionResult> PutUnitTypes([FromBody] List<UnitTypeInfoDTO> unitTypeDTOs)
        {
            if (unitTypeDTOs == null || unitTypeDTOs.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var unitTypeDTO in unitTypeDTOs)
            {
                var unitType = await this.Context.UnitTypes
                    .Include(ut => ut.ProductionCosts)
                    .Include(ut => ut.MaintenaceCosts)
                    .FirstOrDefaultAsync(ut => ut.Id == unitTypeDTO.UnitId);

                if (unitType == null)
                {
                    return NotFound($"Nie znaleziono jednostki o ID {unitTypeDTO.UnitId}.");
                }

                unitType.Name = unitTypeDTO.UnitTypeName;
                unitType.Description = unitTypeDTO.Description;
                unitType.VolunteersNeeded = unitTypeDTO.Quantity;
                unitType.Melee = unitTypeDTO.Melee;
                unitType.Range = unitTypeDTO.Range;
                unitType.Defense = unitTypeDTO.Defense;
                unitType.Speed = unitTypeDTO.Speed;
                unitType.Morale = unitTypeDTO.Morale;
                unitType.IsNaval = unitTypeDTO.IsNaval;

                var updatedProductionCosts = unitTypeDTO.ProductionCost.GroupBy(pc => pc.ResourceId).Select(pc => new ProductionCost
                {
                    UnitTypeId = unitType.Id.Value,
                    ResourceId = pc.Key,
                    Amount = pc.Sum(x => x.Amount),
                }).ToList();

                this.Context.ProductionCosts.RemoveRange(unitType.ProductionCosts);
                this.Context.ProductionCosts.AddRange(updatedProductionCosts);

                var updatedMaintenaceCosts = unitTypeDTO.ConsumedResources.GroupBy(mc => mc.ResourceId).Select(mc => new MaintenaceCosts
                {
                    UnitTypeId = unitType.Id.Value,
                    ResourceId = mc.Key,
                    Amount = mc.Sum(x => x.Amount),
                }).ToList();

                this.Context.MaintenaceCosts.RemoveRange(unitType.MaintenaceCosts);
                this.Context.MaintenaceCosts.AddRange(updatedMaintenaceCosts);

                this.Context.Entry(unitType).State = EntityState.Modified;
            }

            try
            {
                await this.Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        // POST: api/UnitTypes  
        [HttpPost]
        public async Task<ActionResult<UnitTypeDTO>> PostUnitTypes([FromBody] List<UnitTypeInfoDTO> unitTypeDTOs)
        {
            if (unitTypeDTOs == null || unitTypeDTOs.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var unitTypes = unitTypeDTOs.Select(dto => new UnitType
            {
                Name = dto.UnitTypeName,
                Description = dto.Description,
                VolunteersNeeded = dto.Quantity,
                Melee = dto.Melee,
                Range = dto.Range,
                Defense = dto.Defense,
                Speed = dto.Speed,
                Morale = dto.Morale,
                IsNaval = dto.IsNaval,
            }).ToList();

            this.Context.UnitTypes.AddRange(unitTypes);
            await this.Context.SaveChangesAsync();

            foreach (var unitType in unitTypes)
            {
                var correspondingDTO = unitTypeDTOs.FirstOrDefault(dto => dto.UnitTypeName == unitType.Name);

                if (correspondingDTO != null)
                {
                    var productionCosts = correspondingDTO.ProductionCost.GroupBy(pc => pc.ResourceId).Select(pc => new ProductionCost
                    {
                        UnitTypeId = unitType.Id.Value,
                        ResourceId = pc.Key,
                        Amount = pc.Sum(x => x.Amount),
                    }).ToList();

                    this.Context.ProductionCosts.AddRange(productionCosts);

                    var maintenaceCosts = correspondingDTO.ConsumedResources.GroupBy(mc => mc.ResourceId).Select(mc => new MaintenaceCosts
                    {
                        UnitTypeId = unitType.Id.Value,
                        ResourceId = mc.Key,
                        Amount = mc.Sum(x => x.Amount),
                    }).ToList();

                    this.Context.MaintenaceCosts.AddRange(maintenaceCosts);
                }
            }

            await this.Context.SaveChangesAsync();

            var createdDTOs = unitTypes.Select(ut => new UnitTypeDTO
            {
                UnitId = ut.Id.Value,
                UnitName = ut.Name,
                Description = ut.Description,
                Quantity = ut.VolunteersNeeded,
                Melee = ut.Melee,
                Range = ut.Range,
                Defense = ut.Defense,
                Speed = ut.Speed,
                Morale = ut.Morale,
                IsNaval = ut.IsNaval,
            }).ToList();

            return CreatedAtAction("GetUnitTypes", new { id = createdDTOs[0].UnitId }, createdDTOs);
        }

        // DELETE: api/UnitTypes
        [HttpDelete]
        public async Task<ActionResult> DeleteUnitTypes([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var unitTypes = await this.Context.UnitTypes
                .Include(ut => ut.ProductionCosts)
                .Include(ut => ut.MaintenaceCosts)
                .Where(ut => ids.Contains(ut.Id))
                .ToListAsync();

            if (unitTypes.Count == 0)
            {
                return NotFound("Nie znaleziono jednostek do usunięcia.");
            }

            foreach (var unitType in unitTypes)
            {
                this.Context.ProductionCosts.RemoveRange(unitType.ProductionCosts);
                this.Context.MaintenaceCosts.RemoveRange(unitType.MaintenaceCosts);
            }

            this.Context.UnitTypes.RemoveRange(unitTypes);
            await this.Context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("GetLandUnitTypeInfo/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitTypeInfoDTO>>> GetLandUnitTypeInfo(int? nationId)
        {
            nationId ??= this.NationId;
            var accessibleUnitTypeIds = await this.Context.AccessToUnits
                .Where(atu => atu.NationId == nationId)
                .Select(atu => atu.UnitTypeId)
                .ToListAsync();

            var unitTypes = await this.Context.UnitTypes
                .Where(ut => !ut.IsNaval && accessibleUnitTypeIds.Contains(ut.Id.Value))
                .Include(ut => ut.ProductionCosts)
                    .ThenInclude(pc => pc.Resource)

                .Include(ut => ut.MaintenaceCosts)
                    .ThenInclude(mc => mc.Resource)
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
                ConsumedResources = this.GetConsumedResources(ut),
                ProductionCost = this.GetProductionCost(ut),
            }).ToList();

            return Ok(unitTypeInfoList);
        }

        [HttpGet("GetNavalUnitTypeInfo/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitTypeInfoDTO>>> GetNavalUnitTypeInfo(int? nationId)
        {
            nationId ??= this.NationId;

            var accessibleUnitTypeIds = await this.Context.AccessToUnits
                .Where(atu => atu.NationId == nationId)
                .Select(atu => atu.UnitTypeId)
                .ToListAsync();

            var unitTypes = await this.Context.UnitTypes
                .Where(ut => ut.IsNaval && accessibleUnitTypeIds.Contains(ut.Id.Value))
                .Include(ut => ut.ProductionCosts)
                    .ThenInclude(pc => pc.Resource)

                .Include(ut => ut.MaintenaceCosts)
                    .ThenInclude(mc => mc.Resource)
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
                ConsumedResources = this.GetConsumedResources(ut),
                ProductionCost = this.GetProductionCost(ut),
            }).ToList();

            return Ok(unitTypeInfoList);
        }

        [HttpGet("GetAllLandUnitTypeInfo")]
        public async Task<ActionResult<IEnumerable<UnitTypeInfoDTO>>> GetAllLandUnitTypeInfo()
        {
            var unitTypes = await this.Context.UnitTypes
                .Where(ut => !ut.IsNaval)
                .Include(ut => ut.ProductionCosts)
                .ThenInclude(pc => pc.Resource)
                .Include(ut => ut.MaintenaceCosts)
                .ThenInclude(mc => mc.Resource)
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
                ConsumedResources = this.GetConsumedResources(ut),
                ProductionCost = this.GetProductionCost(ut),
            }).ToList();

            return this.Ok(unitTypeInfoList);
        }

        [HttpGet("GetAllNavalUnitTypeInfo")]
        public async Task<ActionResult<IEnumerable<UnitTypeInfoDTO>>> GetAllNavalUnitTypeInfo()
        {
            var unitTypes = await this.Context.UnitTypes
                .Where(ut => ut.IsNaval)
                .Include(ut => ut.ProductionCosts)
                .ThenInclude(pc => pc.Resource)
                .Include(ut => ut.MaintenaceCosts)
                .ThenInclude(mc => mc.Resource)
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
                ConsumedResources = this.GetConsumedResources(ut),
                ProductionCost = this.GetProductionCost(ut),
            }).ToList();

            return this.Ok(unitTypeInfoList);
        }

        private List<ResourceAmountDto> GetConsumedResources(UnitType unitType)
        {
            return [.. unitType.MaintenaceCosts.Select(mc => new ResourceAmountDto
            {
                ResourceId = mc.ResourceId,
                ResourceName = mc.Resource.Name,
                Amount = mc.Amount,
            })];
        }

        private List<ResourceAmountDto> GetProductionCost(UnitType unitType)
        {
            return [.. unitType.ProductionCosts.Select(pc => new ResourceAmountDto
            {
                ResourceId = pc.ResourceId,
                ResourceName = pc.Resource.Name,
                Amount = pc.Amount,
            })];
        }
    }
}
