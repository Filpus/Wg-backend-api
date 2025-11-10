using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Modifiers")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class ModifiersController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public ModifiersController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = this._gameDbContextFactory.Create(schema);
        }

        // DELETE: api/Modifiers
        [HttpDelete]
        public async Task<ActionResult> DeleteModifiers([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var modifiers = await this._context.Modifiers.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (modifiers.Count == 0)
            {
                return NotFound("Nie znaleziono modyfikatorów do usunięcia.");
            }

            this._context.Modifiers.RemoveRange(modifiers);
            await this._context.SaveChangesAsync();

            return Ok();
        }
    }
}
