using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/MapAccesses")]
    [ApiController]
    public class MapAccessController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int? _nationId;

        public MapAccessController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;

            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            _context = _gameDbContextFactory.Create(schema);
            string nationIdStr = _sessionDataService.GetNation();
            _nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
        }

        [HttpGet]
        public async Task<List<(int, int)>> GetMapAccesses()
        {
            var mapAccesses = await this._context.MapAccesses
                .Select(x => new ValueTuple<int, int>(x.NationId, x.MapId))
                .ToListAsync();

            return mapAccesses;
        }

        [HttpGet("{mapId?}")]
        public async Task<ActionResult<List<MapAccessInfoDTO>>> GetAllMapAccesses(int? mapId)
        {
            if (mapId.HasValue)
            {
                var mapAccesses = await _context.MapAccesses
                   .Include(ma => ma.Map)
                   .Include(ma => ma.Nation)
                   .Where(ma => ma.MapId == mapId)
                   .ToListAsync();
            }
            else
            {
                var mapAccesses = await _context.MapAccesses
                   .Include(ma => ma.Map)
                   .Include(ma => ma.Nation)
                   .ToListAsync();
            }

            var result = await _context.MapAccesses
                .Select(ma => new MapAccessInfoDTO
                {
                    NationId = ma.NationId,
                    MapId = ma.MapId,
                    NationName = ma.Nation.Name,
                    NationImage = ma.Nation != null ? ma.Nation.Flag ?? string.Empty : string.Empty,
                    MapName = ma.Map.Name,
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> PostMapAccesses([FromBody] int nationId, int mapId)
        {
            if (nationId <= 0 || mapId <= 0)
            {
                return this.BadRequest("Inappropriate ID");
            }

            var existingMapAccess = await this._context.MapAccesses
                .FirstOrDefaultAsync(ma => ma.NationId == nationId && ma.MapId == mapId);

            if (existingMapAccess == null)
                return this.BadRequest("Map access already exists");

            var map = await this._context.Maps.FindAsync(mapId);
            if (map == null)
                return this.BadRequest("Map does not exist");

            var nation = await this._context.Nations.FindAsync(nationId);
            if (nation == null)
                return this.BadRequest("Nation does not exist");

            var newMapAccess = new MapAccess
            {
                NationId = nationId,
                MapId = mapId,
            };

            this._context.MapAccesses.Add(newMapAccess);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction(nameof(this.GetMapAccesses), newMapAccess);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteMapAccesses([FromBody] List<(int nationId, int mapId)> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var mapAccesses = await this._context.MapAccesses
                .Where(ma => ids.Any(id => id.nationId == ma.NationId && id.mapId == ma.MapId))
                .ToListAsync();

            if (mapAccesses.Count == 0)
            {
                return this.NotFound("Nie znaleziono dostępu do map do usunięcia.");
            }

            this._context.MapAccesses.RemoveRange(mapAccesses);
            await this._context.SaveChangesAsync();

            return this.Ok();
        }
    }
}
