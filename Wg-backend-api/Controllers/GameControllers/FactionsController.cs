using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Factions")]
    [ApiController]
    public class FactionsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        private int? _nationId;


        public FactionsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        public async Task<ActionResult<IEnumerable<FactionDTO>>> GetFactions(int? id)
        {
            if (id.HasValue)
            {
                var faction = await _context.Factions
                    .Where(f => f.Id == id)
                    .Select(f => new FactionDTO
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Power = f.Power,
                        Agenda = f.Agenda,
                        Contentment = f.Contentment,
                        Color = f.Color
                    })
                    .FirstOrDefaultAsync();

                if (faction == null)
                {
                    return NotFound();
                }
                return Ok(new List<FactionDTO> { faction });
            }
            else
            {
                var factions = await _context.Factions
                    .Select(f => new FactionDTO
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Power = f.Power,
                        Agenda = f.Agenda,
                        Contentment = f.Contentment,
                        Color = f.Color
                    })
                    .ToListAsync();

                return Ok(factions);
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutFactions([FromBody] List<FactionDTO> factionsDto)
        {
            if (factionsDto == null || factionsDto.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var dto in factionsDto)
            {
                var faction = await _context.Factions.FindAsync(dto.Id);
                if (faction == null)
                {
                    return NotFound($"Nie znaleziono frakcji o ID {dto.Id}.");
                }

                faction.Name = dto.Name;
                faction.Power = dto.Power;
                faction.Agenda = dto.Agenda;
                faction.Contentment = dto.Contentment;
                faction.Color = dto.Color;

                _context.Entry(faction).State = EntityState.Modified;
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
        public async Task<ActionResult<FactionDTO>> PostFactions([FromBody] List<FactionDTO> factionsDto)
        {
            if (factionsDto == null || factionsDto.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var factions = factionsDto.Select(dto => new Faction
            {
                Name = dto.Name,
                Power = dto.Power,
                Agenda = dto.Agenda,
                Contentment = dto.Contentment,
                Color = dto.Color
            }).ToList();

            _context.Factions.AddRange(factions);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFactions", new { id = factions[0].Id }, factionsDto);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteFactions([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var factions = await _context.Factions.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (factions.Count == 0)
            {
                return NotFound("Nie znaleziono frakcji do usunięcia.");
            }

            _context.Factions.RemoveRange(factions);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("ByNation/{nationId?}")]
        public async Task<ActionResult<IEnumerable<FactionDTO>>> GetFactionsByNation(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var factions = await _context.Factions
                .Where(f => f.NationId == nationId)
                .Select(f => new FactionDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    Power = f.Power,
                    Agenda = f.Agenda,
                    Contentment = f.Contentment,
                    Color = f.Color
                })
                .ToListAsync();

            if (factions == null || factions.Count == 0)
            {
                return NotFound($"Nie znaleziono frakcji dla państwa o ID {nationId}.");
            }

            return Ok(factions);
        }
    }
}
