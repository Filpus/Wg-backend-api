
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/ProductionShares")]
    [ApiController]
    public class ProductionSharesController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public ProductionSharesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        // GET: api/ProductionShares
        // GET: api/ProductionShares/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<ProductionShare>>> GetProductionShares(int? id)
        {
            if (id.HasValue)
            {
                var productionShare = await _context.ProductionShares.FindAsync(id);
                if (productionShare == null)
                {
                    return NotFound();
                }
                return Ok(new List<ProductionShare> { productionShare });
            }
            else
            {
                return await _context.ProductionShares.ToListAsync();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionShare>>> GetProductionShares()
        {
            return await _context.ProductionShares.ToListAsync();
        }

        // PUT: api/ProductionShares
        [HttpPut]
        public async Task<IActionResult> PutProductionShares([FromBody] List<ProductionShare> productionShares)
        {
            if (productionShares == null || productionShares.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var productionShare in productionShares)
            {
                _context.Entry(productionShare).State = EntityState.Modified;
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

        // POST: api/ProductionShares
        [HttpPost]
        public async Task<ActionResult<ProductionShare>> PostProductionShares([FromBody] List<ProductionShare> productionShares)
        {
            if (productionShares == null || productionShares.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (ProductionShare productionShare in productionShares)
            {
                productionShare.Id = null;
            }

            _context.ProductionShares.AddRange(productionShares);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductionShares", new { id = productionShares[0].Id }, productionShares);
        }

        // DELETE: api/ProductionShares
        [HttpDelete]
        public async Task<ActionResult> DeleteProductionShares([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var productionShares = await _context.ProductionShares.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (productionShares.Count == 0)
            {
                return NotFound("Nie znaleziono udziałów produkcji do usunięcia.");
            }

            _context.ProductionShares.RemoveRange(productionShares);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
