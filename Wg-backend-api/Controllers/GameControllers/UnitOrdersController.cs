using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/UnitOrders")]
    [ApiController]
    public class UnitOrdersController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;

        public UnitOrdersController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;

            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        // GET: api/UnitOrders
        // GET: api/UnitOrders/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<UnitOrder>>> GetUnitOrders(int? id)
        {
            if (id.HasValue)
            {
                var unitOrder = await _context.UnitOrders.FindAsync(id);
                if (unitOrder == null)
                {
                    return NotFound();
                }
                return Ok(new List<UnitOrder> { unitOrder });
            }
            else
            {
                return await _context.UnitOrders.ToListAsync();
            }
        }

        // PUT: api/UnitOrders
        [HttpPut]
        public async Task<IActionResult> PutUnitOrders([FromBody] List<UnitOrder> unitOrders)
        {
            if (unitOrders == null || unitOrders.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var unitOrder in unitOrders)
            {
                _context.Entry(unitOrder).State = EntityState.Modified;
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

        // POST: api/UnitOrders
        [HttpPost]
        public async Task<ActionResult<UnitOrder>> PostUnitOrders([FromBody] List<UnitOrder> unitOrders)
        {
            if (unitOrders == null || unitOrders.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (UnitOrder unitOrder in unitOrders)
            {
                unitOrder.Id = null;
            }

            _context.UnitOrders.AddRange(unitOrders);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnitOrders", new { id = unitOrders[0].Id }, unitOrders);
        }

        // DELETE: api/UnitOrders
        [HttpDelete]
        public async Task<ActionResult> DeleteUnitOrders([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var unitOrders = await _context.UnitOrders.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (unitOrders.Count == 0)
            {
                return NotFound("Nie znaleziono zamówień jednostek do usunięcia.");
            }

            _context.UnitOrders.RemoveRange(unitOrders);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
