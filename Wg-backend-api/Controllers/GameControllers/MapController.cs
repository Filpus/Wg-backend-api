using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Maps")]
    [ApiController]
    public class MapController : Controller
    {
        
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int? _nationId;

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
            string nationIdStr = _sessionDataService.GetNation();
            _nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
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
                return Ok(new List<MapDTO> { new MapDTO { Id = map.Id, Name = map.MapLocation, MapLocation = map.MapLocation, MapIconLocation = map.MapIconLocation } });
            }
            else
            {
                var maps = await _context.Maps
                    .Select(map => new MapDTO { Id = map.Id, Name = map.MapLocation, MapLocation = map.MapLocation, MapIconLocation = map.MapIconLocation })
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
                return StatusCode(500, "B��d podczas aktualizacji.");
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
                    return BadRequest($"Nieobs�ugiwany format pliku. Dopuszczalne rozszerzenia: {string.Join(", ", allowedExtensions)}");

                // 4. Walidacja rozmiaru
                const int maxFileSize = 20 * 1024 * 1024; // 5 MB
                if (mapCreateDTO.ImageFile.Length > maxFileSize)
                    return BadRequest($"Maksymalny dopuszczalny rozmiar pliku to {maxFileSize / 1024 / 1024} MB");

                // 5. Generowanie unikalnej nazwy pliku
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images");

                // 6. Tworzenie folderu je�li nie istnieje
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // 7. Zapis pliku na dysku
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await mapCreateDTO.ImageFile.CopyToAsync(stream);
                }
                // tworznenie ikonki 
                var thumbnailFileName = $"{Path.GetFileNameWithoutExtension(uniqueFileName)}_thumb{fileExtension}";
                var thumbnailPath = Path.Combine(uploadsFolder, thumbnailFileName);

                // zapis minitarku
                using (var image = await Image.LoadAsync(filePath))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(300, 300)
                    }));

                    await image.SaveAsync(thumbnailPath); 
                }

                // 8. Zapis do bazy danych
                var imageUrl = $"/images/{uniqueFileName}";
                var thumbnailUrl = $"/images/{thumbnailFileName}";
                var newMap = new Map
                {
                    Name = mapCreateDTO.Name,
                    MapLocation = imageUrl,
                    MapIconLocation = thumbnailUrl
                };

                _context.Maps.Add(newMap);
                await _context.SaveChangesAsync();

                // 9. Przygotowanie odpowiedzi
                var createdMap = new MapDTO
                {
                    Id = newMap.Id,
                    Name = newMap.Name,
                    MapLocation = $"{Request.Scheme}://{Request.Host}{imageUrl}",
                    MapIconLocation = $"{Request.Scheme}://{Request.Host}{thumbnailUrl}"
                };

                return CreatedAtAction(nameof(GetMaps), new { id = createdMap.Id }, createdMap);
            }
            catch (Exception ex)
            {
                // Logowanie b��du (np. do ILogger)
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Wyst�pi� b��d podczas przetwarzania pliku: {ex.Message}"
                );
            }
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteMaps([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usuni�cia.");
            }

            var maps = await _context.Maps.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (maps.Count == 0)
            {
                return NotFound("Nie znaleziono map do usuni�cia.");
            }

            _context.Maps.RemoveRange(maps);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("nation/maps/{nationId?}")]
        public async Task<ActionResult<IEnumerable<MapDTO>>> GetNationMaps(int? nationId)
        {

            if (nationId == null)
            {
                nationId = _nationId;
            }
            
            var nationMaps = await _context.MapAccesses
                .Where(ma => ma.NationId == nationId)
                .Join(_context.Maps,
                    ma => ma.MapId,
                    map => map.Id,
                    (ma, map) => new MapDTO
                    {
                        Id = map.Id,
                        Name = map.Name, 
                        MapLocation = map.MapLocation,
                        MapIconLocation = map.MapIconLocation
                    })
                .ToListAsync();

            if (!nationMaps.Any())
            {
                return NotFound("Nie znaleziono map dla podanego pa�stwa.");
            }

            return Ok(nationMaps);
        }
    }
}
