using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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

        private class FileUploadResult
        {
            public bool Success { get; set; }

            public string? FilePath { get; set; }

            public string? ThumbnailPath { get; set; }

            public string? ErrorMessage { get; set; }
        }

        public MapController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<MapDTO>>> GetMaps(int? id)
        {
            if (id.HasValue)
            {
                var map = await this._context.Maps.FindAsync(id);
                if (map == null)
                {
                    return this.NotFound();
                }

                return this.Ok(new List<MapDTO> { new MapDTO { Id = map.Id, Name = map.Name, MapLocation = map.MapLocation, MapIconLocation = map.MapIconLocation } });
            }
            else
            {
                var maps = await this._context.Maps
                    .Select(map => new MapDTO { Id = map.Id, Name = map.Name, MapLocation = map.MapLocation, MapIconLocation = map.MapIconLocation })
                    .ToListAsync();
                return this.Ok(maps);
            }
        }

        [HttpPatch]
        public async Task<IActionResult> PatchMaps([FromForm] MapCreateDTO mapDTO)
        {
            if (mapDTO.id == null || (string.IsNullOrWhiteSpace(mapDTO.Name) && mapDTO.ImageFile == null))
            {
                return this.BadRequest("Brak danych do edycji.");
            }

            var map = await this._context.Maps.FindAsync(mapDTO.id);
            if (map == null)
            {
                return this.NotFound($"Mapa o ID {mapDTO.id} nie istnieje.");
            }

            if (mapDTO.ImageFile != null)
            {
                var result = await this.UploadFile(mapDTO.ImageFile, true);
                if (!result.Success)
                {
                    return this.BadRequest(result.ErrorMessage);
                }

                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", map.MapLocation.Replace("/images/", string.Empty));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                var iconPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", map.MapIconLocation.Replace("/images/", string.Empty));

                if (System.IO.File.Exists(iconPath))
                {
                    System.IO.File.Delete(iconPath);
                }

                map.MapLocation = result.FilePath;
                map.MapIconLocation = result.ThumbnailPath;
            }

            if (!string.IsNullOrWhiteSpace(mapDTO.Name))
            {
                map.Name = mapDTO.Name;
            }

            this._context.Entry(map).State = EntityState.Modified;

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return this.StatusCode(500, "B��d podczas aktualizacji.");
            }

            return this.NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<MapDTO>> PostMap([FromForm] MapCreateDTO mapCreateDTO)
        {
            try
            {
                if (mapCreateDTO == null || mapCreateDTO.ImageFile == null || string.IsNullOrWhiteSpace(mapCreateDTO.Name))
                {
                    return this.BadRequest("Brak danych do zapisania");
                }

                var result = this.UploadFile(mapCreateDTO.ImageFile, true);

                if (!result.Result.Success)
                {
                    return this.BadRequest(result.Result.ErrorMessage);
                }

                var newMap = new Map
                {
                    Name = mapCreateDTO.Name,
                    MapLocation = result.Result.FilePath,
                    MapIconLocation = result.Result.ThumbnailPath,
                };

                this._context.Maps.Add(newMap);
                await this._context.SaveChangesAsync();

                var createdMap = new MapDTO
                {
                    Id = newMap.Id,
                    Name = newMap.Name,
                    MapLocation = $"{Request.Scheme}://{Request.Host}{result.Result.FilePath}",
                    MapIconLocation = $"{Request.Scheme}://{Request.Host}{result.Result.ThumbnailPath}",
                };

                return this.CreatedAtAction(nameof(this.GetMaps), new { id = createdMap.Id }, createdMap);
            }
            catch (Exception ex)
            {
                return this.StatusCode(
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
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var maps = await this._context.Maps.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (maps.Count == 0)
            {
                return this.NotFound("Nie znaleziono map do usuni�cia.");
            }

            foreach (var map in maps)
            {
                if (!string.IsNullOrEmpty(map.MapLocation))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", map.MapLocation.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                if (!string.IsNullOrEmpty(map.MapIconLocation))
                {
                    var iconPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", map.MapIconLocation.TrimStart('/'));
                    if (System.IO.File.Exists(iconPath))
                    {
                        System.IO.File.Delete(iconPath);
                    }
                }
            }

            this._context.Maps.RemoveRange(maps);
            await this._context.SaveChangesAsync();

            return this.Ok();
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

        private async Task<FileUploadResult> UploadFile(IFormFile file, bool isThumbnail = false)
        {
            if (file.Length == 0)
            {
                return new FileUploadResult { Success = false, ErrorMessage = "No file was uploaded." };
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"This file type is not supported. Allowed extensions: {string.Join(", ", allowedExtensions)}",
                };
            }

            const int maxFileSize = 20 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Max file size is {maxFileSize / 1024 / 1024} MB.",
                };
            }

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string thumbnailFileName = string.Empty;
            if (isThumbnail)
            {
                thumbnailFileName = $"{Path.GetFileNameWithoutExtension(uniqueFileName)}_thumb{fileExtension}";
                var thumbnailPath = Path.Combine(uploadsFolder, thumbnailFileName);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                using (var image = await Image.LoadAsync(filePath))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(300, 300),
                    }));

                    await image.SaveAsync(thumbnailPath);
                }
            }

            //$"{Request.Scheme}://{Request.Host}{imageUrl}",
            var pathFile = $"/images/{uniqueFileName}";

            return new FileUploadResult { Success = true, FilePath = pathFile, ThumbnailPath = isThumbnail ? $"/images/{thumbnailFileName}" : null };
        }

        private bool IsNationDependency(int id)
        {
            var hasDependencies = this._context.AccessToUnits.Any(e => e.NationId == id) ||
                                this._context.Actions.Any(e => e.NationId == id) ||
                                this._context.OwnedResources.Any(e => e.NationId == id) ||
                                this._context.Armies.Any(e => e.NationId == id) ||
                                this._context.Factions.Any(e => e.NationId == id) ||
                                this._context.Localisations.Any(e => e.NationId == id) ||
                                this._context.RelatedEvents.Any(e => e.NationId == id) ||
                                this._context.TradeAgreements.Any(e => e.OfferingNationId == id || e.ReceivingNationId == id) ||
                                this._context.UnitOrders.Any(e => e.NationId == id);

            return hasDependencies;
        }

    }
}
