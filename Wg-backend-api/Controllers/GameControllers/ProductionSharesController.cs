
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
