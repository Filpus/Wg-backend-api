using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Armies")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class ArmiesController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int? _nationId;

        public ArmiesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = this._gameDbContextFactory.Create(schema);
            string nationIdStr = this._sessionDataService.GetNation();
            this._nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult> GetArmy(int? id)
        {
            if (id == null)
            {
                var armies = await this._context.Armies
                    .Select(a => new ArmiesDTO
                    {
                        ArmyId = a.Id.Value,
                        ArmyName = a.Name,
                        LocationId = a.LocationId,
                        NationId = a.NationId,
                        IsNaval = a.IsNaval,
                    })
                    .ToListAsync();

                return Ok(armies);
            }

            var army = await this._context.Armies
                .Select(a => new ArmiesDTO
                {
                    ArmyId = a.Id.Value,
                    ArmyName = a.Name,
                    LocationId = a.LocationId,
                    NationId = a.NationId,
                    IsNaval = a.IsNaval,
                })
                .FirstOrDefaultAsync(a => a.ArmyId == id);

            if (army == null)
            {
                return NotFound("Army not found.");
            }

            return Ok(army);
        }

        [HttpPost]
        public async Task<ActionResult> CreateArmy([FromBody] CreateArmyDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Name) || dto.LocationId == null)
            {
                return BadRequest(this.ModelState);
            }

            if (this._nationId == null)
            {
                return BadRequest("Nation ID is missing in session.");
            }

            var existingArmy = await this._context.Armies
                .FirstOrDefaultAsync(a => a.Name == dto.Name);
            if (existingArmy != null)
            {
                return BadRequest("Army with the same name already exists.");
            }

            var army = new Army
            {
                Name = dto.Name,
                LocationId = dto.LocationId,
                NationId = this._nationId.Value,
                IsNaval = dto.IsNaval,
            };

            this._context.Armies.Add(army);
            await this._context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArmy), new { id = army.Id }, army);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateArmy([FromBody] PutArmyDTO dto)
        {
            var army = await this._context.Armies.FindAsync(dto.Id);

            if (army == null || dto.LocationId == null)
            {
                return NotFound("Army not found.");
            }

            var existingArmy = await this._context.Armies
                .FirstOrDefaultAsync(a => a.Name == dto.Name && a.Id != dto.Id);
            if (existingArmy != null)
            {
                return BadRequest("Army with the same name already exists.");
            }

            var hasArmyTroops = await this._context.Troops.Where(t => t.ArmyId == army.Id).FirstOrDefaultAsync();

            if (hasArmyTroops != null)
            {
                var unitType = await this._context.UnitTypes.FindAsync(hasArmyTroops.UnitTypeId);
                if (unitType != null && unitType.IsNaval != dto.IsNaval)
                {
                    return BadRequest("Cannot change army type when it has troops.");
                }
            }

            army.Name = dto.Name ?? army.Name;
            army.NationId = dto.NationId ?? army.NationId;
            army.LocationId = dto.LocationId ?? army.LocationId;
            army.IsNaval = dto.IsNaval;

            await this._context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteArmy([FromBody] int id)
        {
            var army = await this._context.Armies.Where(r => r.Id == id).FirstOrDefaultAsync();

            if (army == null)
            {
                return NotFound("Army not found.");
            }

            if (army.LocationId == null)
            {
                return BadRequest("Cannot delete barracks or docks.");
            }

            var barracksOrDocks = await this._context.Armies
                .Where(a => a.LocationId == null && a.NationId == army.NationId && a.IsNaval == army.IsNaval)
                .FirstOrDefaultAsync();

            if (barracksOrDocks == null)
            {
                return BadRequest("Cannot delete the only barracks or docks of the nation.");
            }

            var armyTroops = await this._context.Troops
                .Where(t => t.ArmyId == army.Id)
                .ToListAsync();
            foreach (var troop in armyTroops)
            {
                troop.ArmyId = (int)barracksOrDocks.Id;
            }

            this._context.Armies.Remove(army);
            await this._context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("GetNavalArmiesByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<ArmiesInfoDTO>>> GetNavalArmiesByNationId(int? nationId)
        {
            nationId ??= this._nationId;

            var navalArmies = await this._context.Armies
                .Where(a => a.NationId == nationId && a.IsNaval && a.LocationId != null)
                .Include(a => a.Troops)
                    .ThenInclude(t => t.UnitType)
                .Select(a => new ArmiesInfoDTO
                {
                    ArmyId = a.Id.Value,
                    ArmyName = a.Name,
                    Location = a.Location.Name,
                    LocationId = a.LocationId,
                    Nation = a.Nation.Id.ToString(),
                    IsNaval = a.IsNaval,
                    Units = a.Troops
                        .GroupBy(t => t.UnitTypeId)
                        .Select(g => new TroopsAgregatedDTO
                        {
                            Id = g.First().UnitTypeId,
                            UnitTypeName = g.First().UnitType.Name,
                            Quantity = g.Sum(t => t.Quantity),
                            TroopCount = g.Count(),
                        })
                        .ToList(),
                    TotalStrength = a.Troops.Sum(t => t.Quantity),
                })
                .ToListAsync();

            return Ok(navalArmies);
        }

        [HttpGet("GetLandArmiesByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<ArmiesInfoDTO>>> GetLandArmiesByNationId(int? nationId)
        {
            nationId ??= this._nationId;

            var armies = await this._context.Armies
                .Where(a => a.NationId == nationId && !a.IsNaval && a.LocationId != null)
                .Include(a => a.Troops)
                    .ThenInclude(t => t.UnitType)
                .Select(a => new ArmiesInfoDTO
                {
                    ArmyId = a.Id.Value,
                    ArmyName = a.Name,
                    Location = a.Location.Name,
                    LocationId = a.LocationId,
                    Nation = a.Nation.Id.ToString(),
                    IsNaval = a.IsNaval,
                    Units = a.Troops
                        .GroupBy(t => t.UnitTypeId)
                        .Select(g => new TroopsAgregatedDTO
                        {
                            Id = g.First().UnitTypeId,
                            UnitTypeName = g.First().UnitType.Name,
                            Quantity = g.Sum(t => t.Quantity),
                            TroopCount = g.Count(),
                        })
                        .ToList(),
                    TotalStrength = a.Troops.Sum(t => t.Quantity),
                })
                .ToListAsync();

            return Ok(armies);
        }

        [HttpGet("GetManpowerInfoByNationId/{nationId?}")]
        public async Task<ActionResult<ManpowerInfoDTO>> GetManpowerInfoByNationId(int? nationId)
        {
            nationId ??= this._nationId;

            if (nationId == null)
            {
                return BadRequest("Brak ID państwa.");
            }

            // Total manpower: Sum of volunteers from all populations by their social groups
            var totalManpower = await this._context.Populations
                .Where(p => p.Location.NationId == nationId)
                .SumAsync(p => p.SocialGroup.Volunteers);

            // Manpower in land armies: Sum of all troop quantities in land armies
            var manpowerInLandArmies = await this._context.Armies
                .Where(a => a.NationId == nationId && !a.IsNaval && a.LocationId != null)
                .SumAsync(a => a.Troops.Sum(t => t.Quantity));

            // Manpower in naval armies: Sum of all troop quantities in naval armies
            var manpowerInNavalArmies = await this._context.Armies
                .Where(a => a.NationId == nationId && a.IsNaval && a.LocationId != null)
                .SumAsync(a => a.Troops.Sum(t => t.Quantity));

            // Recruiting land manpower: Sum of all units in recruitment for land armies
            var recruitingLandManpower = await this._context.UnitOrders
               .Where(uo => uo.NationId == nationId && !uo.UnitType.IsNaval)
               .SumAsync(uo => uo.Quantity * uo.UnitType.VolunteersNeeded);

            // Recruiting naval manpower: Sum of all units in recruitment for naval armies
            var recruitingNavalManpower = await this._context.UnitOrders
               .Where(uo => uo.NationId == nationId && uo.UnitType.IsNaval)
               .SumAsync(uo => uo.Quantity * uo.UnitType.VolunteersNeeded);

            var manpowerInfo = new ManpowerInfoDTO
            {
                TotalMappower = totalManpower,
                AvailableManpower = totalManpower - (manpowerInLandArmies + manpowerInNavalArmies + recruitingLandManpower + recruitingNavalManpower),
                RecruitingLandManpower = recruitingLandManpower,
                RecruitingNavalManpower = recruitingNavalManpower,
                ManpowerInLandArmies = manpowerInLandArmies,
                ManpowerInNavalArmies = manpowerInNavalArmies,
            };

            return Ok(manpowerInfo);
        }

        [HttpGet("GetBarracksAndDocks/{nationId?}")]
        public async Task<ActionResult<IEnumerable<ArmiesInfoDTO>>> GetBarracksAndDocksByNationId(int? nationId)
        {
            nationId ??= this._nationId;

            var armies = await this._context.Armies
                .Where(a => a.NationId == nationId && a.LocationId == null)
                .Include(a => a.Troops)
                    .ThenInclude(t => t.UnitType)
                .Select(a => new ArmiesInfoDTO
                {
                    ArmyId = a.Id.Value,
                    ArmyName = a.Name,
                    Nation = a.Nation.Id.ToString(),
                    IsNaval = a.IsNaval,
                    Units = a.Troops
                        .GroupBy(t => t.UnitTypeId)
                        .Select(g => new TroopsAgregatedDTO
                        {
                            Id = g.First().UnitTypeId,
                            UnitTypeName = g.First().UnitType.Name,
                            Quantity = g.Sum(t => t.Quantity),
                            TroopCount = g.Count(),
                        })
                        .ToList(),
                    TotalStrength = a.Troops.Sum(t => t.Quantity),
                })
                .ToListAsync();

            return Ok(armies);
        }

        [HttpPost("ChangeTroopsNumberInArmy")]
        public async Task<ActionResult> ChangeTroopsNumberInArmy([FromBody] TroopAmountDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Brak danych wejściowych.");
            }

            if (dto.ArmyId <= 0 || dto.UnitTypeId <= 0 || dto.Amount < 0)
            {
                return BadRequest("Nieprawidłowe dane: sprawdź ArmmyId, UnitTypeId oraz Amount.");
            }

            var army = await this._context.Armies.FindAsync(dto.ArmyId);
            if (army == null)
            {
                return NotFound("Army not found.");
            }

            var unitType = await this._context.UnitTypes.FindAsync(dto.UnitTypeId);
            if (unitType == null)
            {
                return BadRequest("Unit type not found.");
            }

            var troops = await this._context.Troops
                .Where(t => t.ArmyId == army.Id && t.UnitTypeId == dto.UnitTypeId)
                .OrderBy(t => t.Id)
                .ToListAsync();

            var currentCount = troops.Count;

            if (currentCount == dto.Amount)
            {
                return Ok();
            }

            if (currentCount > dto.Amount)
            {
                var needToRemove = currentCount - dto.Amount;
                for (var i = troops.Count - 1; i >= 0 && needToRemove > 0; i--)
                {
                    var troop = troops[i];
                    this._context.Troops.Remove(troop);
                    needToRemove--;
                }
            }
            else 
            {
                var needToAdd = dto.Amount - currentCount;
                for (var i = 0; i < needToAdd; i++)
                {
                    var newTroop = new Troop
                    {
                        ArmyId = (int)army.Id,
                        UnitTypeId = dto.UnitTypeId,
                        Quantity = unitType.VolunteersNeeded,
                    };

                    this._context.Troops.Add(newTroop);
                }
            }

            await this._context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("ReasignTroopsAmountInAmount")]
        public async Task<ActionResult> ReasignTroopsAmountInAmount([FromBody] TroopAmountDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Brak danych wejściowych.");
            }

            if (dto.ArmyId <= 0 || dto.UnitTypeId <= 0 || dto.Amount < 0)
            {
                return BadRequest("Nieprawidłowe dane: sprawdź ArmmyId, UnitTypeId oraz Amount.");
            }

            var army = await this._context.Armies.FindAsync(dto.ArmyId);
            if (army == null)
            {
                return NotFound("Army not found.");
            }

            var unitType = await this._context.UnitTypes.FindAsync(dto.UnitTypeId);
            if (unitType == null)
            {
                return BadRequest("Unit type not found.");
            }

            var targetArmy = await this._context.Armies
                .Where(a => a.NationId == army.NationId && a.Id == dto.TargetArmyId && a.IsNaval == army.IsNaval)
                .FirstOrDefaultAsync();
            if (targetArmy == null)
            {
                return BadRequest("Nie znaleziono odpowiednich koszar/stoczni dla tego państwa i typu.");
            }

            var troopsInArmy = await this._context.Troops
                .Where(t => t.ArmyId == army.Id && t.UnitTypeId == dto.UnitTypeId)
                .OrderBy(t => t.Id)
                .ToListAsync();

            var currentCount = troopsInArmy.Count;

            if (currentCount == dto.Amount)
            {
                return Ok();
            }

            if (currentCount > dto.Amount)
            {
                var needToMove = currentCount - dto.Amount;
                for (var i = troopsInArmy.Count - 1; i >= 0 && needToMove > 0; i--)
                {
                    var troop = troopsInArmy[i];
                    troop.ArmyId = (int)targetArmy.Id;
                    needToMove--;
                }
            }
            else
            {
                var needToMove = dto.Amount - currentCount;

                var troopsInBarracks = await this._context.Troops
                    .Where(t => t.ArmyId == targetArmy.Id && t.UnitTypeId == dto.UnitTypeId)
                    .OrderBy(t => t.Id)
                    .Take(needToMove)
                    .ToListAsync();

                foreach (var troop in troopsInBarracks)
                {
                    troop.ArmyId = (int)army.Id;
                    needToMove--;
                }

                for (var i = 0; i < needToMove; i++)
                {
                    var newTroop = new Troop
                    {
                        ArmyId = (int)army.Id,
                        UnitTypeId = dto.UnitTypeId,
                        Quantity = unitType.VolunteersNeeded,
                    };

                    this._context.Troops.Add(newTroop);
                }
            }

            await this._context.SaveChangesAsync();
            return Ok();
        }



    }
}
