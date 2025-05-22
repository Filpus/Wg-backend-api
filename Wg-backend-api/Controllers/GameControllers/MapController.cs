using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Maps")]
    [ApiController]
    public class MapController : Controller
    {
        
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public MapController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        public async Task<ActionResult<MapDTO>> PostMap([FromForm] MapCreateDTO mapCreateDTO)
        {
            try
            {
                // 1. Walidacja podstawowa
                if (mapCreateDTO == null)
                    return BadRequest("Brak danych do zapisania");

                if (string.IsNullOrWhiteSpace(mapCreateDTO.Name))
                    return BadRequest("Nazwa mapy jest wymagana");

                // 2. Walidacja pliku
                if (mapCreateDTO.ImageFile == null || mapCreateDTO.ImageFile.Length == 0)
                    return BadRequest("Nie wybrano pliku obrazu");

                // 3. Walidacja rozszerzenia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(mapCreateDTO.ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest($"Nieobs³ugiwany format pliku. Dopuszczalne rozszerzenia: {string.Join(", ", allowedExtensions)}");

                // 4. Walidacja rozmiaru
                const int maxFileSize = 20 * 1024 * 1024; // 5 MB
                if (mapCreateDTO.ImageFile.Length > maxFileSize)
                    return BadRequest($"Maksymalny dopuszczalny rozmiar pliku to {maxFileSize / 1024 / 1024} MB");

                // 5. Generowanie unikalnej nazwy pliku
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images");

                // 6. Tworzenie folderu jeœli nie istnieje
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // 7. Zapis pliku na dysku
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await mapCreateDTO.ImageFile.CopyToAsync(stream);
                }

                // 8. Zapis do bazy danych
                var imageUrl = $"/images/{uniqueFileName}";
                var newMap = new Map
                {
                    Name = mapCreateDTO.Name,
                    MapLocation = imageUrl,
                };

                _context.Maps.Add(newMap);
                await _context.SaveChangesAsync();

                // 9. Przygotowanie odpowiedzi
                var createdMap = new MapDTO
                {
                    Id = newMap.Id,
                    Name = newMap.Name,
                    MapLocation = $"{Request.Scheme}://{Request.Host}{imageUrl}"
                };

                return CreatedAtAction(nameof(GetMaps), new { id = createdMap.Id }, createdMap);
            }
            catch (Exception ex)
            {
                // Logowanie b³êdu (np. do ILogger)
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Wyst¹pi³ b³¹d podczas przetwarzania pliku: {ex.Message}"
                );
            }
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
