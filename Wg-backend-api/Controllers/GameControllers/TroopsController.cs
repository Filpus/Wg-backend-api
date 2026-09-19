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
    [AuthorizeGameRole("GameMaster", "Player")]
    public class TroopsController : GameControllerBase
    {
        public TroopsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        // GET: api/Troops/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<TroopDTO>>> GetTroops(int? id)
        {
            if (id.HasValue)
            {
                var troop = await this.Context.Troops.FindAsync(id);
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
                var troops = await this.Context.Troops.ToListAsync();
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

            var nationId = this.NationId.Value;
            if (nationId == -1)
            {
                return BadRequest("Brak ID nacji w sesji.");
            }

            var troopsToInsert = new List<Troop>();

            foreach (var dto in troopDTOs)
            {
                var unitType = await this.Context.UnitTypes.FindAsync(dto.UnitTypeId);

                if (unitType == null)
                {
                    return BadRequest($"Unit Type ID {dto.UnitTypeId} does not exist.");
                }

                var army = await this.Context.Armies.FindAsync(dto.ArmyId);

                if (dto.ArmyId == null)
                {
                    var barracks_or_docks = await this.Context.Armies
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

            this.Context.Troops.AddRange(troopsToInsert);
            await this.Context.SaveChangesAsync();

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

            var nationId = this.NationId.Value;
            if (nationId == -1)
            {
                return BadRequest("Brak ID nacji w sesji.");
            }

            using var transaction = await this.Context.Database.BeginTransactionAsync();
            try
            {
                foreach (var dto in troopDTOs)
                {
                    var currentTroop = await this.Context.Troops
                        .FirstOrDefaultAsync(t => t.ArmyId == dto.CurrentArmyId && t.UnitTypeId == dto.UnitTypeId);
                    var currentArmy = await this.Context.Armies.FindAsync(dto.CurrentArmyId);
                    var destinationArmy = await this.Context.Armies.FindAsync(dto.ArmyId);
                    var troopInDestinationArmy = await this.Context.Troops
                        .FirstOrDefaultAsync(t => t.ArmyId == dto.ArmyId && t.UnitTypeId == dto.UnitTypeId);
                    var unitType = await this.Context.UnitTypes.FindAsync(dto.UnitTypeId);

                    if (currentTroop == null || currentArmy == null || destinationArmy == null || unitType == null)
                    {
                        return NotFound($"Troop or Army does not exsit.");
                    }

                    if (currentArmy.NationId != nationId || destinationArmy.NationId != nationId)
                    {
                        return BadRequest("One of the armies does not belong to your nation.");
                    }

                    if (destinationArmy.IsNaval != unitType.IsNaval)
                    {
                        return BadRequest("Cannot assign naval unit to land army or vice versa.");
                    }

                    if (dto.Quantity < 0)
                    {
                        return BadRequest("Quantity cannot be negative.");
                    }

                    var barracks_or_docks = await this.Context.Armies
                        .Where(a => a.NationId == nationId && a.IsNaval == unitType.IsNaval && a.LocationId == null)
                        .FirstOrDefaultAsync();

                    if (barracks_or_docks == null)
                    {
                        return BadRequest("No barracks/docks found.");
                    }

                    var existingTroopInBarracksOrDocks = await this.Context.Troops
                        .FirstOrDefaultAsync(t => t.ArmyId == barracks_or_docks.Id && t.UnitTypeId == dto.UnitTypeId);

                    // same army
                    if (destinationArmy.Id == currentArmy.Id)
                    {
                        // no change
                        if (currentTroop.Quantity == dto.Quantity)
                        {
                            continue;
                        }

                        // if quantity chaned move to barracs/docks
                        if (dto.Quantity < currentTroop.Quantity)
                        {
                            var difference = currentTroop.Quantity - dto.Quantity;
                            if (existingTroopInBarracksOrDocks != null)
                            {
                                existingTroopInBarracksOrDocks.Quantity += difference;
                            }
                            else
                            {
                                this.Context.Troops.Add(new Troop
                                {
                                    ArmyId = (int)barracks_or_docks.Id,
                                    UnitTypeId = dto.UnitTypeId,
                                    Quantity = difference,
                                });
                            }

                            if (dto.Quantity == 0)
                            {
                                this.Context.Troops.Remove(currentTroop);
                            }
                            else
                            {
                                currentTroop.Quantity = dto.Quantity;
                            }
                        }
                        else
                        {
                            // or return error
                            continue;
                        }
                    }
                    else
                    {
                        // move to different army
                        if (destinationArmy.Id != barracks_or_docks.Id)
                        {
                            if (dto.Quantity == currentTroop.Quantity)
                            {
                                if (troopInDestinationArmy != null)
                                {
                                    troopInDestinationArmy.Quantity += currentTroop.Quantity;
                                    this.Context.Troops.Remove(currentTroop);
                                    continue;
                                }
                                else
                                {
                                    currentTroop.ArmyId = (int)destinationArmy.Id;
                                    continue;
                                }
                            }
                            else if (dto.Quantity < currentTroop.Quantity)
                            {
                                var difference = currentTroop.Quantity - dto.Quantity;

                                // move difference to barracks/docks
                                if (existingTroopInBarracksOrDocks != null)
                                {
                                    existingTroopInBarracksOrDocks.Quantity += difference;
                                }
                                else
                                {
                                    this.Context.Troops.Add(new Troop
                                    {
                                        ArmyId = (int)barracks_or_docks.Id,
                                        UnitTypeId = dto.UnitTypeId,
                                        Quantity = difference,
                                    });
                                }

                                // move to destination army
                                if (troopInDestinationArmy != null)
                                {
                                    troopInDestinationArmy.Quantity += dto.Quantity;
                                    this.Context.Troops.Remove(currentTroop);
                                    continue;
                                }
                                else
                                {
                                    currentTroop.ArmyId = (int)destinationArmy.Id;
                                    currentTroop.Quantity = dto.Quantity;
                                    continue;
                                }
                            }
                            else
                            {
                                // or return error
                                continue;
                            }
                        }
                        else
                        {
                            // moving to barracks or docks
                            if (existingTroopInBarracksOrDocks != null)
                            {
                                existingTroopInBarracksOrDocks.Quantity += currentTroop.Quantity;
                                this.Context.Troops.Remove(currentTroop);
                                continue;
                            }
                            else
                            {
                                currentTroop.ArmyId = (int)barracks_or_docks.Id;
                                continue;
                            }
                        }
                    }
                }

                await this.Context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            await this.Context.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("changeArmy")]
        public async Task<ActionResult> ChangeTroopsArmy([FromBody] List<TroopChangeArmyDTO> data)
        {
            if (data == null || data.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var dto in data)
            {

                var army = await this.Context.Armies.FindAsync(dto.NewArmyId);
                if (army == null)
                {
                    return BadRequest($"Army ID {dto.NewArmyId} does not exist.");
                }

                var troop = await this.Context.Troops.Where(t => t.UnitTypeId == dto.UnitId && t.ArmyId == army.Id).FirstOrDefaultAsync();

                if (troop == null)
                {
                    return NotFound($"Troop with Unit ID {dto.UnitId} does not exsit.");
                }

                var unitType = await this.Context.UnitTypes.FindAsync(troop.UnitTypeId);
                if (unitType == null)
                {
                    return BadRequest($"Unit Type ID {troop.UnitTypeId} does not exist.");
                }

                if (army.IsNaval != unitType.IsNaval)
                {
                    return BadRequest("Cannot assign naval unit to land army or vice versa.");
                }

                var accesToUnit = await this.Context.AccessToUnits
                    .FirstOrDefaultAsync(a => a.NationId == army.NationId && a.UnitTypeId == troop.UnitTypeId);

                if (accesToUnit == null || accesToUnit.NationId != army.NationId)
                {
                    return BadRequest("The nation's army does not have access to this unit type.");
                }

                troop.ArmyId = dto.NewArmyId;
            }

            await this.Context.SaveChangesAsync();
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

            var troops = await this.Context.Troops.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (troops.Count == 0)
            {
                return NotFound("Nie znaleziono jednostek do usunięcia.");
            }

            this.Context.Troops.RemoveRange(troops);
            await this.Context.SaveChangesAsync();

            return Ok();
        }
    }
}
