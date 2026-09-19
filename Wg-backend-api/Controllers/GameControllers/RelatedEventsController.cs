using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [AuthorizeGameRole("GameMaster", "Player")]
    public class RelatedEventsController : GameControllerBase
    {
        public RelatedEventsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        // DELETE: api/RelatedEvents
        [HttpDelete]
        public async Task<ActionResult> DeleteRelatedEvents([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var relatedEvents = await this.Context.RelatedEvents.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (relatedEvents.Count == 0)
            {
                return NotFound("Nie znaleziono wydarzeń do usunięcia.");
            }

            this.Context.RelatedEvents.RemoveRange(relatedEvents);
            await this.Context.SaveChangesAsync();

            return Ok();
        }
    }
}
