using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Nations")]
    [ApiController]
    public class NationController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public NationController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<NationDTO>>> GetNations(int? id)
        {
            if (id.HasValue)
            {
                var nation = await _context.Nations.FindAsync(id);
                if (nation == null)
                {
                    return NotFound();
                }
                return Ok(new List<NationDTO>
                {
                    new NationDTO
                    {
                        Id = nation.Id,
                        Name = nation.Name,
                        ReligionId = nation.ReligionId,
                        CultureId = nation.CultureId,
                        AssignmentIsActive = false // Placeholder, adjust logic as needed  
                    }
                });
            }
            else
            {
                var nations = await _context.Nations.ToListAsync();
                return Ok(nations.Select(n => new NationDTO
                {
                    Id = n.Id,
                    Name = n.Name,
                    ReligionId = n.ReligionId,
                    CultureId = n.CultureId,
                    AssignmentIsActive = false // Placeholder, adjust logic as needed  
                }));
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NationDTO>>> GetNations()
        {
            var nations = await _context.Nations.ToListAsync();
            return Ok(nations.Select(n => new NationDTO
            {
                Id = n.Id,
                Name = n.Name,
                ReligionId = n.ReligionId,
                CultureId = n.CultureId,
                AssignmentIsActive = false // Placeholder, adjust logic as needed  
            }));
        }

        [HttpGet("other-nations")]
        public async Task<List<NationBaseInfoDTO>> GetOtherNations()
        {
            var nationId = _sessionDataService.GetNation();

            if (string.IsNullOrEmpty(nationId))
            {
                return new List<NationBaseInfoDTO>();
            }
            int id = int.Parse(nationId);

            return await _context.Nations
                .Where(n => n.Id != id)
                .Select(n => new NationBaseInfoDTO
                {
                    Id = n.Id,
                    Name = n.Name
                })
                .ToListAsync();
        }

        [HttpPut]
        public async Task<IActionResult> PutNations([FromBody] List<NationDTO> nations)
        {
            if (nations == null || nations.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var nationDto in nations)
            {
                var nation = await _context.Nations.FindAsync(nationDto.Id);
                if (nation == null)
                {
                    return NotFound($"Nie znaleziono państwa o ID {nationDto.Id}.");
                }

                nation.Name = nationDto.Name;
                nation.ReligionId = nationDto.ReligionId;
                nation.CultureId = nationDto.CultureId;

                _context.Entry(nation).State = EntityState.Modified;
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
        public async Task<ActionResult<NationDTO>> PostNations([FromBody] List<NationDTO> nations)
        {
            if (nations == null || nations.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var newNations = nations.Select(nationDto => new Nation
            {
                Name = nationDto.Name,
                ReligionId = nationDto.ReligionId,
                CultureId = nationDto.CultureId
            }).ToList();

            _context.Nations.AddRange(newNations);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNations", new { id = newNations[0].Id }, newNations.Select(n => new NationDTO
            {
                Id = n.Id,
                Name = n.Name,
                ReligionId = n.ReligionId,
                CultureId = n.CultureId,
                AssignmentIsActive = false // Placeholder, adjust logic as needed  
            }));
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteNations([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var nations = await _context.Nations.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (nations.Count == 0)
            {
                return NotFound("Nie znaleziono państwa do usunięcia.");
            }

            _context.Nations.RemoveRange(nations);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
