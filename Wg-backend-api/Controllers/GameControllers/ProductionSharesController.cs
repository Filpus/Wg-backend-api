using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/ProductionShares")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class ProductionSharesController : GameControllerBase
    {
        public ProductionSharesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        // DELETE: api/ProductionShares
        [HttpDelete]
        public async Task<ActionResult> DeleteProductionShares([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var productionShares = await this.Context.ProductionShares.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (productionShares.Count == 0)
            {
                return NotFound("Nie znaleziono udziałów produkcji do usunięcia.");
            }

            this.Context.ProductionShares.RemoveRange(productionShares);
            await this.Context.SaveChangesAsync();

            return Ok();
        }
    }
}
