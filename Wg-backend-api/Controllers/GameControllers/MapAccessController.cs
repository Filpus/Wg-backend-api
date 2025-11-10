using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/MapAccesses")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class MapAccessController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int? _nationId;

        public MapAccessController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = this._gameDbContextFactory.Create(schema);
            string nationIdStr = this._sessionDataService.GetNation();
            this._nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
        }

        [HttpGet("{mapId?}")]
        public async Task<ActionResult<List<MapAccessInfoDTO>>> GetAllMapAccesses(int? mapId)
        {
            var mapAccesses = new List<MapAccessInfoDTO>();
            if (mapId.HasValue)
            {
                mapAccesses = await this._context.MapAccesses
                   .Include(ma => ma.Map)
                   .Include(ma => ma.Nation)
                   .Where(ma => ma.MapId == mapId)
                    .Select(ma => new MapAccessInfoDTO
                    {
                        NationId = ma.NationId,
                        MapId = ma.MapId,
                        NationName = ma.Nation.Name,
                        NationImage = ma.Nation != null ? ma.Nation.Flag ?? string.Empty : string.Empty,
                        MapName = ma.Map.Name,
                    })
                   .ToListAsync();
            }
            else
            {
                mapAccesses = await this._context.MapAccesses
                   .Include(ma => ma.Map)
                   .Include(ma => ma.Nation)
                    .Select(ma => new MapAccessInfoDTO
                    {
                        NationId = ma.NationId,
                        MapId = ma.MapId,
                        NationName = ma.Nation.Name,
                        NationImage = ma.Nation != null ? ma.Nation.Flag ?? string.Empty : string.Empty,
                        MapName = ma.Map.Name,
                    })
                   .ToListAsync();
            }

            return this.Ok(mapAccesses);
        }

        [HttpPost]
        public async Task<ActionResult> PostMapAccesses([FromBody] List<MapAccessCreateDTO> ids)
        {
            if (ids == null || !ids.Any())
            {
                return this.BadRequest("No map access entries provided.");
            }

            var newMapAccesses = new List<MapAccess>();
            foreach (var mapaccess in ids)
            {
                int nationId = mapaccess.NationId;
                int mapId = mapaccess.MapId;
                if (nationId < 0 || mapId < 0)
                {
                    return this.BadRequest("Inappropriate ID");
                }

                var existingMapAccess = await this._context.MapAccesses
                    .FirstOrDefaultAsync(ma => ma.NationId == nationId && ma.MapId == mapId);
                Console.WriteLine(existingMapAccess);
                if (existingMapAccess != null)
                {
                    return this.BadRequest("Map access already exists");
                }

                var map = await this._context.Maps.FirstOrDefaultAsync(map => map.Id == mapId);
                if (map == null)
                {
                    return this.BadRequest("Map does not exist");
                }

                var nation = await this._context.Nations.FirstOrDefaultAsync(map => map.Id == nationId);
                if (nation == null)
                {
                    return this.BadRequest("Nation does not exist");
                }

                var newMapAccess = new MapAccess
                {
                    NationId = nationId,
                    MapId = mapId,
                };

                newMapAccesses.Add(newMapAccess);
                this._context.MapAccesses.Add(newMapAccess);
            }

            await this._context.SaveChangesAsync();

            return this.CreatedAtAction(nameof(this.GetAllMapAccesses), new {});
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteMapAccesses([FromBody] List<MapAccessCreateDTO> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            foreach (var item in ids)
            {
                if (item.NationId < 0 || item.MapId < 0)
                {
                    return this.BadRequest("Nieprawidłowe ID.");
                }
            }

            foreach (var id in ids)
            {
                var mapAccess = await this._context.MapAccesses
                    .FirstOrDefaultAsync(ma => ma.NationId == id.NationId && ma.MapId == id.MapId);

                if (mapAccess != null)
                {
                    this._context.MapAccesses.Remove(mapAccess);
                }
            }

            await this._context.SaveChangesAsync();

            return this.Ok();
        }
    }
}
