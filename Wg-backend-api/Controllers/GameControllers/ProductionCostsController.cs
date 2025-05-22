using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/ProductionCosts")]
    [ApiController]
    public class ProductionCostsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public ProductionCostsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        // GET: api/ProductionCosts
        // GET: api/ProductionCosts/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<ProductionCost>>> GetProductionCosts(int? id)
        {
            if (id.HasValue)
            {
                var productionCost = await _context.ProductionCosts.FindAsync(id);
                if (productionCost == null)
                {
                    return NotFound();
                }
                return Ok(new List<ProductionCost> { productionCost });
            }
            else
            {
                return await _context.ProductionCosts.ToListAsync();
            }
        }

        // PUT: api/ProductionCosts
        [HttpPut]
        public async Task<IActionResult> PutProductionCosts([FromBody] List<ProductionCost> productionCosts)
        {
            if (productionCosts == null || productionCosts.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var productionCost in productionCosts)
            {
                _context.Entry(productionCost).State = EntityState.Modified;
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

        // POST: api/ProductionCosts
        [HttpPost]
        public async Task<ActionResult<ProductionCost>> PostProductionCosts([FromBody] List<ProductionCost> productionCosts)
        {
            if (productionCosts == null || productionCosts.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (ProductionCost productionCost in productionCosts)
            {
                productionCost.Id = null;
            }

            _context.ProductionCosts.AddRange(productionCosts);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductionCosts", new { id = productionCosts[0].Id }, productionCosts);
        }

        // DELETE: api/ProductionCosts
        [HttpDelete]
        public async Task<ActionResult> DeleteProductionCosts([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var productionCosts = await _context.ProductionCosts.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (productionCosts.Count == 0)
            {
                return NotFound("Nie znaleziono kosztów produkcji do usunięcia.");
            }

            _context.ProductionCosts.RemoveRange(productionCosts);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
