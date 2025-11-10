using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/UsedResources")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class UsedResourcesController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public UsedResourcesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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


        // DELETE: api/UsedResources
        [HttpDelete]
        public async Task<ActionResult> DeleteUsedResources([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var usedResources = await _context.UsedResources.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (usedResources.Count == 0)
            {
                return NotFound("Nie znaleziono zasobów do usunięcia.");
            }

            _context.UsedResources.RemoveRange(usedResources);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
