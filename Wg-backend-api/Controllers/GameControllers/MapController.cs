using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Maps")]
    [ApiController]
    public class MapController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;

        public MapController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;
            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<MapDTO>>> GetMaps(int? id)
        {
            if (id.HasValue)
            {
                var map = await _context.Maps.FindAsync(id);
                if (map == null)
                {
                    return NotFound();
                }
                return Ok(new List<MapDTO> { new MapDTO { Id = map.Id, Name = map.MapLocation, MapLocation = map.MapLocation } });
            }
            else
            {
                var maps = await _context.Maps
                    .Select(map => new MapDTO { Id = map.Id, Name = map.MapLocation, MapLocation = map.MapLocation })
                    .ToListAsync();
                return Ok(maps);
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutMaps([FromBody] List<MapDTO> mapDTOs)
        {
            if (mapDTOs == null || mapDTOs.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var mapDTO in mapDTOs)
            {
                var map = await _context.Maps.FindAsync(mapDTO.Id);
                if (map == null)
                {
                    return NotFound($"Mapa o ID {mapDTO.Id} nie istnieje.");
                }

                map.MapLocation = mapDTO.MapLocation;
                _context.Entry(map).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "B³¹d podczas aktualizacji.");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<MapDTO>> PostMaps([FromBody] List<MapDTO> mapDTOs)
        {
            if (mapDTOs == null || mapDTOs.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var maps = mapDTOs.Select(dto => new Map
            {
                MapLocation = dto.MapLocation
            }).ToList();

            _context.Maps.AddRange(maps);
            await _context.SaveChangesAsync();

            var createdDTOs = maps.Select(map => new MapDTO
            {
                Id = map.Id,
                Name = map.MapLocation,
                MapLocation = map.MapLocation
            }).ToList();

            return CreatedAtAction("GetMaps", new { id = createdDTOs.First().Id }, createdDTOs);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteMaps([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usuniêcia.");
            }

            var maps = await _context.Maps.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (maps.Count == 0)
            {
                return NotFound("Nie znaleziono map do usuniêcia.");
            }

            _context.Maps.RemoveRange(maps);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("nation/{nationId}/maps")]
        public async Task<ActionResult<IEnumerable<MapDTO>>> GetNationMaps(int nationId)
        {
            var nationMaps = await _context.MapAccesses
                .Where(ma => _context.Localisations
                    .Any(loc => loc.NationId == nationId && loc.Id == ma.MapId))
                .Join(_context.Maps,
                    ma => ma.MapId,
                    map => map.Id,
                    (ma, map) => new MapDTO
                    {
                        Id = map.Id,
                        Name = map.Name, // Example name, adjust as needed  
                        MapLocation = map.MapLocation
                    })
                .ToListAsync();

            if (!nationMaps.Any())
            {
                return NotFound("Nie znaleziono map dla podanego pañstwa.");
            }

            return Ok(nationMaps);
        }
    }
}
