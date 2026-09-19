using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/UsedResources")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class UsedResourcesController : GameControllerBase
    {
        public UsedResourcesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        // DELETE: api/UsedResources
        [HttpDelete]
        public async Task<ActionResult> DeleteUsedResources([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var usedResources = await this.Context.UsedResources.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (usedResources.Count == 0)
            {
                return NotFound("Nie znaleziono zasobów do usunięcia.");
            }

            this.Context.UsedResources.RemoveRange(usedResources);
            await this.Context.SaveChangesAsync();

            return Ok();
        }
    }
}
