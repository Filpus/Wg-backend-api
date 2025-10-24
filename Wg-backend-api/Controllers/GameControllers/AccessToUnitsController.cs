using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/AccessToUnits")]
    [ApiController]
    public class AccessToUnitsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public AccessToUnitsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        [HttpDelete]
        public async Task<ActionResult> DeleteAccessToUnits([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var accessToUnits = await _context.AccessToUnits.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (accessToUnits.Count == 0)
            {
                return NotFound("Nie znaleziono dostępu do jednostek do usunięcia.");
            }

            _context.AccessToUnits.RemoveRange(accessToUnits);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("ByNation/{nationId}")]
        public async Task<ActionResult<IEnumerable<UnitTypeAccessInfoDTO>>> GetAccessByNation(int nationId)
        {
            if (nationId <= 0)
            {
                return BadRequest("Nieprawidłowe ID państwa.");
            }

            var list = await _context.AccessToUnits
                .Where(a => a.NationId == nationId)
                .Select(a => new UnitTypeAccessInfoDTO
                {
                    NationId = a.NationId,
                    UnitTypeId = a.UnitTypeId,
                    NationName = a.Nation.Name,
                    UnitTypeName = a.UnitType.Name
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("ByUnitType/{unitTypeId}")]
        public async Task<ActionResult<IEnumerable<AccessToUnit>>> GetAccessByUnitType(int unitTypeId)
        {
            if (unitTypeId <= 0)
            {
                return BadRequest("Nieprawidłowe ID typu jednostki.");
            }

            var list = await _context.AccessToUnits
                .Where(a => a.UnitTypeId == unitTypeId)
                .Select(a => new UnitTypeAccessInfoDTO
                {
                    NationId = a.NationId,
                    UnitTypeId = a.UnitTypeId,
                    NationName = a.Nation.Name,
                    UnitTypeName = a.UnitType.Name
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult> CreateAccess([FromBody] List<UnitTypeAccessCreateDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest("Brak danych do utworzenia.");
            }

            var accessToUnits = dtos
                .Where(dto => dto.NationId > 0 && dto.UnitTypeId > 0)
                .Select(dto => new AccessToUnit
                {
                    NationId = dto.NationId,
                    UnitTypeId = dto.UnitTypeId,
                })
                .ToList();



            _context.AccessToUnits.AddRange(accessToUnits);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> EditAccessList([FromBody] List<AccessToUnit> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest("Brak danych do aktualizacji.");
            }

            var ids = dtos.Select(dto => dto.Id).Where(id => id.HasValue).Select(id => id.Value).ToList();
            var existingRecords = await _context.AccessToUnits.Where(a => ids.Contains(a.Id.Value)).ToListAsync();

            if (existingRecords.Count != dtos.Count)
            {
                return NotFound("Niektóre rekordy do aktualizacji nie zostały znalezione.");
            }

            foreach (var dto in dtos)
            {
                var existing = existingRecords.FirstOrDefault(e => e.Id == dto.Id);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(dto);
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Rekordy zostały zaktualizowane.");
        }

    }
}
