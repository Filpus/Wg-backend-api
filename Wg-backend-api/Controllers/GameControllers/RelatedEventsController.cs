using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/RelatedEvents")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class RelatedEventsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public RelatedEventsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;

            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }
        }
  

        // DELETE: api/RelatedEvents
        [HttpDelete]
        public async Task<ActionResult> DeleteRelatedEvents([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var relatedEvents = await _context.RelatedEvents.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (relatedEvents.Count == 0)
            {
                return NotFound("Nie znaleziono wydarzeń do usunięcia.");
            }

            _context.RelatedEvents.RemoveRange(relatedEvents);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
