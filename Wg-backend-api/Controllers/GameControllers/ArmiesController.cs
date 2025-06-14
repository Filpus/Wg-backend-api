﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Armies")]
    [ApiController]
    public class ArmiesController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int? _nationId;

        public ArmiesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Army>>> GetArmies(int? id)
        {
            if (id.HasValue)
            {
                var army = await _context.Armies.FindAsync(id);
                if (army == null)
                {
                    return NotFound();
                }
                return Ok(new List<Army> { army });
            }
            else
            {
                return await _context.Armies.ToListAsync();
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutArmies([FromBody] List<Army> armies)
        {
            if (armies == null || armies.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var army in armies)
            {
                _context.Entry(army).State = EntityState.Modified;
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

        [HttpPost]
        public async Task<ActionResult<Army>> PostArmies([FromBody] List<Army> armies)
        {
            if (armies == null || armies.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (Army army in armies)
            {
                army.Id = null;
            }

            _context.Armies.AddRange(armies);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArmies", new { id = armies[0].Id }, armies);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteArmies([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var armies = await _context.Armies.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (armies.Count == 0)
            {
                return NotFound("Nie znaleziono armii do usunięcia.");
            }

            _context.Armies.RemoveRange(armies);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("GetNavalArmiesByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<ArmiesInfoDTO>>> GetNavalArmiesByNationId(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }

            var navalArmies = await _context.Armies
                .Where(a => a.NationId == nationId && a.IsNaval)
                .Include(a => a.Troops)
                    .ThenInclude(t => t.UnitType)
                .Select(a => new ArmiesInfoDTO
                {
                    ArmyId = a.Id.Value,
                    ArmyName = a.Name,
                    Location = a.Location.Name,
                    Nation = a.Nation.Id.ToString(),
                    IsNaval = a.IsNaval,
                    Units = a.Troops
                        .GroupBy(t => t.UnitTypeId)
                        .Select(g => new TroopsAgregatedDTO
                        {
                            UnitTypeName = g.First().UnitType.Name,
                            Quantity = g.Sum(t => t.Quantity),
                            TroopCount = g.Count()
                        })
                        .ToList(),
                    TotalStrength = a.Troops.Sum(t => t.Quantity)
                })
                .ToListAsync();

            return Ok(navalArmies);
        }

        [HttpGet("GetLandArmiesByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<ArmiesInfoDTO>>> GetLandArmiesByNationId(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }

            var armies = await _context.Armies
                .Where(a => a.NationId == nationId && !a.IsNaval)
                .Include(a => a.Troops)
                    .ThenInclude(t => t.UnitType)
                .Select(a => new ArmiesInfoDTO
                {
                    ArmyId = a.Id.Value,
                    ArmyName = a.Name,
                    Location = a.Location.Name,
                    Nation = a.Nation.Id.ToString(),
                    IsNaval = a.IsNaval,
                    Units = a.Troops
                        .GroupBy(t => t.UnitTypeId)
                        .Select(g => new TroopsAgregatedDTO
                        {
                            UnitTypeName = g.First().UnitType.Name,
                            Quantity = g.Sum(t => t.Quantity),
                            TroopCount = g.Count()
                        })
                        .ToList(),
                    TotalStrength = a.Troops.Sum(t => t.Quantity)
                })
                .ToListAsync();

            return Ok(armies);
        }

        [HttpGet("GetManpowerInfoByNationId/{nationId?}")]
        public async Task<ActionResult<ManpowerInfoDTO>> GetManpowerInfoByNationId(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }

            if (nationId == null)
            {
                return BadRequest("Brak ID państwa.");
            }

            // Total manpower: Sum of volunteers from all populations by their social groups
            var totalManpower = await _context.Populations
                .Where(p => p.Location.NationId == nationId)
                .SumAsync(p => p.SocialGroup.Volunteers);

            // Manpower in land armies: Sum of all troop quantities in land armies
            var manpowerInLandArmies = await _context.Armies
                .Where(a => a.NationId == nationId && !a.IsNaval)
                .SumAsync(a => a.Troops.Sum(t => t.Quantity));

            // Manpower in naval armies: Sum of all troop quantities in naval armies
            var manpowerInNavalArmies = await _context.Armies
                .Where(a => a.NationId == nationId && a.IsNaval)
                .SumAsync(a => a.Troops.Sum(t => t.Quantity));

            // Recruiting land manpower: Sum of all units in recruitment for land armies
            var recruitingLandManpower = await _context.UnitOrders
               .Where(uo => uo.NationId == nationId && !uo.UnitType.IsNaval)
               .SumAsync(uo => uo.Quantity * uo.UnitType.VolunteersNeeded);

            // Recruiting naval manpower: Sum of all units in recruitment for naval armies  
            var recruitingNavalManpower = await _context.UnitOrders
               .Where(uo => uo.NationId == nationId && uo.UnitType.IsNaval)
               .SumAsync(uo => uo.Quantity * uo.UnitType.VolunteersNeeded);

            var manpowerInfo = new ManpowerInfoDTO
            {
                TotalMappower = totalManpower,
                AvailableManpower = totalManpower - (manpowerInLandArmies + manpowerInNavalArmies + recruitingLandManpower+ recruitingNavalManpower),
                RecruitingLandManpower = recruitingLandManpower,
                RecruitingNavalManpower = recruitingNavalManpower,
                ManpowerInLandArmies = manpowerInLandArmies,
                ManpowerInNavalArmies = manpowerInNavalArmies
            };

            return Ok(manpowerInfo);
        }


       

    }
}
