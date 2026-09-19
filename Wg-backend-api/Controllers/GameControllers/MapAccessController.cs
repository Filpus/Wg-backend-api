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
    [AuthorizeGameRole("GameMaster", "Player")]
    public class MapAccessController : GameControllerBase
    {
        public MapAccessController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        [HttpGet("{mapId?}")]
        public async Task<ActionResult<List<MapAccessInfoDTO>>> GetAllMapAccesses(int? mapId)
        {
            var mapAccesses = new List<MapAccessInfoDTO>();
            if (mapId.HasValue)
            {
                mapAccesses = await this.Context.MapAccesses
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
                mapAccesses = await this.Context.MapAccesses
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
                int nationId = 0;
                if (mapaccess.NationId == null)
                {
                    nationId = (int)this.NationId;
                }
                else
                {
                    nationId = (int)mapaccess.NationId;

                }

                int mapId = mapaccess.MapId;
                if (nationId < 0 || mapId < 0)
                {
                    return this.BadRequest("Inappropriate ID");
                }

                var existingMapAccess = await this.Context.MapAccesses
                    .FirstOrDefaultAsync(ma => ma.NationId == nationId && ma.MapId == mapId);
                Console.WriteLine(existingMapAccess);
                if (existingMapAccess != null)
                {
                    return this.BadRequest("Map access already exists");
                }

                var map = await this.Context.Maps.FirstOrDefaultAsync(map => map.Id == mapId);
                if (map == null)
                {
                    return this.BadRequest("Map does not exist");
                }

                var nation = await this.Context.Nations.FirstOrDefaultAsync(map => map.Id == nationId);
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
                this.Context.MapAccesses.Add(newMapAccess);
            }

            await this.Context.SaveChangesAsync();

            return this.CreatedAtAction(nameof(this.GetAllMapAccesses), new { });
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
                item.NationId ??= this.NationId;

                if (item.NationId < 0 || item.MapId < 0)
                {
                    return this.BadRequest("Nieprawidłowe ID.");
                }
            }

            foreach (var id in ids)
            {
                var mapAccess = await this.Context.MapAccesses
                    .FirstOrDefaultAsync(ma => ma.NationId == id.NationId && ma.MapId == id.MapId);

                if (mapAccess != null)
                {
                    this.Context.MapAccesses.Remove(mapAccess);
                }
            }

            await this.Context.SaveChangesAsync();

            return this.Ok();
        }
        [HttpGet("MissingAccess/{nationId?}")]
        public async Task<ActionResult<IEnumerable<MapAccessInfoDTO>>> GetMissingMapAccess(int? nationId)
        {
            nationId ??= this.NationId;

            if (nationId is null or <= 0)
            {
                return BadRequest("Nieprawidłowe ID państwa.");
            }

            var allMaps = await this.Context.Maps.ToListAsync();

            var nationAccess = await this.Context.MapAccesses
                .Where(ma => ma.NationId == nationId)
                .Select(ma => ma.MapId)
                .ToListAsync();

            var missingAccess = allMaps
                .Where(map => !nationAccess.Contains((int)map.Id))
                .Select(map => new MapAccessInfoDTO
                {
                    MapId = (int)map.Id,
                    MapName = map.Name,
                    NationId = nationId.Value,
                    NationName = this.Context.Nations.FirstOrDefault(n => n.Id == nationId)?.Name ?? string.Empty,
                })
                .ToList();

            return Ok(missingAccess);
        }

    }
}
