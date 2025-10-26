using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Troops")]
    [ApiController]
    public class TroopsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public TroopsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        // GET: api/Troops/5  
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<TroopDTO>>> GetTroops(int? id)
        {
            if (id.HasValue)
            {
                var troop = await _context.Troops.FindAsync(id);
                if (troop == null)
                {
                    return NotFound();
                }
                var troopDTO = new TroopDTO
                {
                    Id = troop.Id,
                    UnitTypeId = troop.UnitTypeId,
                    ArmyId = troop.ArmyId,
                    Quantity = troop.Quantity
                };
                return Ok(new List<TroopDTO> { troopDTO });
            }
            else
            {
                var troops = await _context.Troops.ToListAsync();
                var troopDTOs = troops.Select(t => new TroopDTO
                {
                    Id = t.Id,
                    UnitTypeId = t.UnitTypeId,
                    ArmyId = t.ArmyId,
                    Quantity = t.Quantity
                }).ToList();
                return Ok(troopDTOs);
            }
        }

        // PUT: api/Troops  
        [HttpPut]
        public async Task<IActionResult> PutTroops([FromBody] List<TroopDTO> troopDTOs)
        {
            if (troopDTOs == null || troopDTOs.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var troopDTO in troopDTOs)
            {
                var troop = new Troop
                {
                    Id = troopDTO.Id,
                    UnitTypeId = troopDTO.UnitTypeId,
                    ArmyId = troopDTO.ArmyId,
                    Quantity = troopDTO.Quantity
                };
                _context.Entry(troop).State = EntityState.Modified;
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

        // POST: api/Troops  
        [HttpPost]
        public async Task<ActionResult<List<TroopDTO>>> PostTroops([FromBody] List<TroopDTO> troopDTOs)
        {
            if (troopDTOs == null || troopDTOs.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var troops = troopDTOs.Select(t => new Troop
            {
                Id = null,
                UnitTypeId = t.UnitTypeId,
                ArmyId = t.ArmyId,
                Quantity = t.Quantity
            }).ToList();

            _context.Troops.AddRange(troops);
            await _context.SaveChangesAsync();

            var createdTroopDTOs = troops.Select(t => new TroopDTO
            {
                Id = t.Id,
                UnitTypeId = t.UnitTypeId,
                ArmyId = t.ArmyId,
                Quantity = t.Quantity
            }).ToList();

            return CreatedAtAction("GetTroops", new { id = createdTroopDTOs[0].Id }, createdTroopDTOs);
        }

        // DELETE: api/Troops
        [HttpDelete]
        public async Task<ActionResult> DeleteTroops([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var troops = await _context.Troops.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (troops.Count == 0)
            {
                return NotFound("Nie znaleziono jednostek do usunięcia.");
            }

            _context.Troops.RemoveRange(troops);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
