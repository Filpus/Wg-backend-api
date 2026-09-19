using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Modifiers")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class ModifiersController : GameControllerBase
    {
        public ModifiersController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        // DELETE: api/Modifiers
        [HttpDelete]
        public async Task<ActionResult> DeleteModifiers([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var modifiers = await this.Context.Modifiers.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (modifiers.Count == 0)
            {
                return NotFound("Nie znaleziono modyfikatorów do usunięcia.");
            }

            this.Context.Modifiers.RemoveRange(modifiers);
            await this.Context.SaveChangesAsync();

            return Ok();
        }
    }
}
