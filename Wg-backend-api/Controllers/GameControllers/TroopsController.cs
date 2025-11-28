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
                    Quantity = troop.Quantity,
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
                    Quantity = t.Quantity,
                }).ToList();
                return Ok(troopDTOs);
            }
        }

        // POST: api/Troops
        [HttpPost]
        public async Task<ActionResult<List<TroopDTO>>> PostTroops([FromBody] List<CreteTroopDTO> troopDTOs)
        {
            if (troopDTOs == null || troopDTOs.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var nationId = int.Parse(this._sessionDataService.GetNation());
            if (nationId == -1)
            {
                return BadRequest("Brak ID nacji w sesji.");
            }

            var troopsToInsert = new List<Troop>();

            foreach (var dto in troopDTOs)
            {
                var unitType = await this._context.UnitTypes.FindAsync(dto.UnitTypeId);

                if (unitType == null)
                {
                    return BadRequest($"Unit Type ID {dto.UnitTypeId} does not exist.");
                }

                var army = await this._context.Armies.FindAsync(dto.ArmyId);

                if (dto.ArmyId == null)
                {
                    var barracks_or_docks = await this._context.Armies
                        .Where(a => a.NationId == nationId && a.IsNaval == unitType.IsNaval && a.LocationId == null)
                        .FirstOrDefaultAsync();
                    army = barracks_or_docks;
                }

                if (army == null || army.Id == null)
                {
                    return BadRequest($"Army ID {dto.ArmyId} does not exist.");
                }

                if (army.IsNaval != unitType.IsNaval)
                {
                    return BadRequest("Cannot assign naval unit to land army or vice versa.");
                }

                if (dto.Quantity < 0)
                {
                    return BadRequest("Quantity cannot be negative.");
                }

                troopsToInsert.Add(new Troop
                {
                    UnitTypeId = dto.UnitTypeId,
                    ArmyId = (int)army.Id,
                    Quantity = dto.Quantity,
                });
            }

            this._context.Troops.AddRange(troopsToInsert);
            await this._context.SaveChangesAsync();

            var result = troopsToInsert.Select(t => new TroopDTO
            {
                Id = t.Id,
                UnitTypeId = t.UnitTypeId,
                ArmyId = t.ArmyId,
                Quantity = t.Quantity,
            }).ToList();

            return CreatedAtAction(nameof(GetTroops), new { id = result[0].Id }, result);
        }

        // PUT: api/Troops
        [HttpPut]
        public async Task<ActionResult> PutTroops([FromBody] List<TroopUpdateDTO> troopDTOs)
        {
            if (troopDTOs == null || troopDTOs.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            var nationId = int.Parse(this._sessionDataService.GetNation());
            if (nationId == -1)
            {
                return BadRequest("Brak ID nacji w sesji.");
            }

            foreach (var dto in troopDTOs)
            {
                var troop = await this._context.Troops.FindAsync(dto.Id);

                if (troop == null)
                {
                    return NotFound($"Troop ID {dto.Id} does not exsit.");
                }

                var unitType = await this._context.UnitTypes.FindAsync(dto.UnitTypeId);

                if (unitType == null)
                {
                    return BadRequest($"Unit Type ID {dto.UnitTypeId} does not exist.");
                }

                var army = await this._context.Armies.FindAsync(dto.ArmyId ?? troop.ArmyId);
                if (dto.ArmyId == null)
                {
                    var barracks_or_docks = await this._context.Armies
                        .Where(a => a.NationId == nationId && unitType.IsNaval && a.LocationId == null)
                        .FirstOrDefaultAsync();
                    if (barracks_or_docks != null)
                    {
                        army = barracks_or_docks;
                    }
                }

                if (army.IsNaval != unitType.IsNaval)
                {
                    return BadRequest("Cannot assign naval unit to land army or vice versa.");
                }

                if (dto.Quantity < 0)
                {
                    return BadRequest("Quantity cannot be negative.");
                }

                troop.UnitTypeId = dto.UnitTypeId;
                troop.ArmyId = army.Id ?? troop.ArmyId;
                troop.Quantity = dto.Quantity;
            }

            await this._context.SaveChangesAsync();
            return Ok();
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
