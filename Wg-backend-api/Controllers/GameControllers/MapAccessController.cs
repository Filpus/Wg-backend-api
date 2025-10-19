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



        [HttpDelete]
        public async Task<ActionResult> DeleteMapAccesses([FromBody] List<(int nationId, int mapId)> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usuniêcia.");
            }

            var mapAccesses = await _context.MapAccesses
                .Where(ma => ids.Any(id => id.nationId == ma.NationId && id.mapId == ma.MapId))
                .ToListAsync();


            if (mapAccesses.Count == 0)
            {
                return NotFound("Nie znaleziono dostêpu do map do usuniêcia.");
            }

            _context.MapAccesses.RemoveRange(mapAccesses);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
