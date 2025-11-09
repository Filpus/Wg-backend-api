using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/AccessToUnits")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class AccessToUnitsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int? _nationId;

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
            string nationIdStr = _sessionDataService.GetNation();
            _nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);

        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAccessToUnits([FromBody] List<int?> ids)
        {

            if (_nationId == null)
            {
                return BadRequest("Brak ID państwa w sesji.");
            }

            foreach (int unitTypeId in ids)
            {
                if (unitTypeId <= 0)
                {
                    return BadRequest("Nieprawidłowe ID typu jednostki.");
                }

                var accessToUnits = await _context.AccessToUnits
                    .Where(a => a.NationId == _nationId && a.UnitTypeId == unitTypeId)
                    .ToListAsync();

                if (accessToUnits.Count == 0)
                {
                    return NotFound("Nie znaleziono dostępu do jednostek do usunięcia.");
                }

                _context.AccessToUnits.RemoveRange(accessToUnits);
                await _context.SaveChangesAsync();
            }

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
                .Select(dto => new AccessToUnit
                {
                    NationId = dto.NationId > 0 ? (int)dto.NationId : _nationId ?? throw new InvalidOperationException("Brak ID państwa w sesji."),
                    UnitTypeId = dto.UnitTypeId,
                })
                .ToList();



            _context.AccessToUnits.AddRange(accessToUnits);
            await _context.SaveChangesAsync();

            return Ok();
        }

        
        [HttpGet("LandMissingAccess/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitTypeDTO>>> GetLandMissingAccess(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }

            var allUnitTypes = await _context.UnitTypes.ToListAsync();

            var nationAccess = await _context.AccessToUnits
                .Where(a => a.NationId == nationId)
                .Select(a => a.UnitTypeId)
                .ToListAsync();

            // Oblicz różnicę zbiorów
            var missingAccess = allUnitTypes
                .Where(ut => !nationAccess.Contains((int)ut.Id))
                .Where(ut => !ut.IsNaval)
                .Select(ut => new UnitTypeDTO
                {
                    UnitId = (int)ut.Id,
                    UnitName = ut.Name,
                    Quantity = 0,
                    Melee = ut.Melee,
                    Range = ut.Range,
                    Defense = ut.Defense,
                    Speed = ut.Speed,
                    Morale = ut.Morale,
                    IsNaval = ut.IsNaval
                })
                .ToList();

            return Ok(missingAccess);
        }

        [HttpGet("NavalMissingAccess/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitTypeDTO>>> GetNavalMissingAccess(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }

            var allUnitTypes = await _context.UnitTypes.ToListAsync();

            var nationAccess = await _context.AccessToUnits
                .Where(a => a.NationId == nationId)
                .Select(a => a.UnitTypeId)
                .ToListAsync();

            // Oblicz różnicę zbiorów
            var missingAccess = allUnitTypes
                .Where(ut => !nationAccess.Contains((int)ut.Id))
                .Where(ut => !ut.IsNaval)
                .Select(ut => new UnitTypeDTO
                {
                    UnitId = (int)ut.Id,
                    UnitName = ut.Name,
                    Quantity = 0,
                    Melee = ut.Melee,
                    Range = ut.Range,
                    Defense = ut.Defense,
                    Speed = ut.Speed,
                    Morale = ut.Morale,
                    IsNaval = ut.IsNaval
                })
                .ToList();

            return Ok(missingAccess);


        }
    }
}