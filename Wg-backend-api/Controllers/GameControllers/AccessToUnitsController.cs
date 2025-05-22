using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
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

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<AccessToUnit>>> GetAccessToUnits(int? id)
        {
            if (id.HasValue)
            {
                var accessToUnit = await _context.AccessToUnits.FindAsync(id);
                if (accessToUnit == null)
                {
                    return NotFound();
                }
                return Ok(new List<AccessToUnit> { accessToUnit });
            }
            else
            {
                return await _context.AccessToUnits.ToListAsync();
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutAccessToUnits([FromBody] List<AccessToUnit> accessToUnits)
        {
            if (accessToUnits == null || accessToUnits.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var accessToUnit in accessToUnits)
            {
                _context.Entry(accessToUnit).State = EntityState.Modified;
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
        public async Task<ActionResult<AccessToUnit>> PostAccessToUnits([FromBody] List<AccessToUnit> accessToUnits)
        {
            if (accessToUnits == null || accessToUnits.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (AccessToUnit accessToUnit in accessToUnits)
            {
                accessToUnit.Id = null;
            }

            _context.AccessToUnits.AddRange(accessToUnits);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccessToUnits", new { id = accessToUnits[0].Id }, accessToUnits);
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
    }
}
