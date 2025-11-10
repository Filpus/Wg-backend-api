using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Troops")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class TroopsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public TroopsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        // GET: api/Troops/5  
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<TroopDTO>>> GetTroops(int? id)
        {
            if (id.HasValue)
            {
                var troop = await this._context.Troops.FindAsync(id);
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
                var troops = await this._context.Troops.ToListAsync();
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
                this._context.Entry(troop).State = EntityState.Modified;
            }

            try
            {
                await this._context.SaveChangesAsync();
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

            this._context.Troops.AddRange(troops);
            await this._context.SaveChangesAsync();

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

            var troops = await this._context.Troops.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (troops.Count == 0)
            {
                return NotFound("Nie znaleziono jednostek do usunięcia.");
            }

            this._context.Troops.RemoveRange(troops);
            await this._context.SaveChangesAsync();

            return Ok();
        }
    }
}
