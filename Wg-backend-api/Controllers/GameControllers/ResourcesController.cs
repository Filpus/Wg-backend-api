namespace Wg_backend_api.Controllers.GameControllers
{
    // For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Wg_backend_api.Data;
    using Wg_backend_api.DTO;
    using Wg_backend_api.Logic.Modifiers.Processors;
    using Wg_backend_api.Logic.Resources;
    using Wg_backend_api.Models;
    using Wg_backend_api.Services;

    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int _nationId;

        public ResourcesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
            this._nationId = int.Parse(nationIdStr);
        }

        // GET: api/Resources
        // GET: api/Resources/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<ResourceDto>>> GetResources(int? id)
        {
            if (id.HasValue)
            {
                var resource = await this._context.Resources.FindAsync(id);
                if (resource == null)
                {
                    return this.NotFound();
                }

                var resourceDto = new ResourceDto
                {
                    Id = resource.Id.Value,
                    Name = resource.Name,
                    IsMain = resource.IsMain,
                    Icon = resource.Icon,
                };
                return this.Ok(new List<ResourceDto> { resourceDto }); // Zwraca pojedynczy zasób w liście
            }
            else
            {
                var resources = await this._context.Resources.ToListAsync();
                var resourceDtos = resources.Select(r => new ResourceDto
                {
                    Id = r.Id.Value,
                    Name = r.Name,
                    IsMain = r.IsMain,
                    Icon = r.Icon,
                }).ToList();
                return this.Ok(resourceDtos); // Zwraca wszystkie zasoby
            }
        }

        // PUT: api/Resources
        [HttpPut]
        public async Task<IActionResult> PutResources([FromBody] List<ResourceDto> resourceDtos)
        {
            if (resourceDtos == null || resourceDtos.Count == 0)
            {
                return this.BadRequest("Brak danych do edycji.");
            }

            // check the name is between 1 and 64 characters
            foreach (var resourceDto in resourceDtos)
            {
                if (string.IsNullOrWhiteSpace(resourceDto.Name) || resourceDto.Name.Length > 64)
                {
                    return this.BadRequest("Nazwa zasobu jest niepoprawnej długości.");
                }
            }

            foreach (var resourceDto in resourceDtos)
            {
                var resource = await this._context.Resources.FindAsync(resourceDto.Id);
                if (resource == null)
                {
                    return this.NotFound($"Nie znaleziono zasobu o ID: {resourceDto.Id}");
                }

                resource.Name = resourceDto.Name;
                resource.IsMain = resourceDto.IsMain;
                resource.Icon = resourceDto.Icon;

                this._context.Entry(resource).State = EntityState.Modified;
            }

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return this.StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        // POST: api/Resources
        [HttpPost]
        public async Task<ActionResult<List<ResourceDto>>> PostResources([FromForm] List<CreateResourceDto> resources)
        {
            if (resources == null || !resources.Any())
            {
                return this.BadRequest("Brak danych do zapisania.");
            }

            // check the name is between 1 and 64 characters
            foreach (var dto in resources)
            {
                if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 64)
                {
                    return this.BadRequest("Nazwa zasobu jest niepoprawnej długości.");
                }
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            const int maxFileSize = 5 * 1024 * 1024;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "resources", "icons");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var savedResources = new List<Resource>();

            foreach (var dto in resources)
            {
                string? iconPath = null;

                if (dto.IconFile != null)
                {
                    var extension = Path.GetExtension(dto.IconFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return this.BadRequest($"Nieprawidłowe rozszerzenie pliku: {dto.IconFile.FileName}");
                    }

                    if (dto.IconFile.Length > maxFileSize)
                    {
                        return this.BadRequest($"Plik {dto.IconFile.FileName} przekracza maksymalny rozmiar {maxFileSize / 1024 / 1024} MB");
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.IconFile.CopyToAsync(stream);
                    }

                    iconPath = $"/images/{uniqueFileName}";
                }

                savedResources.Add(new Resource
                {
                    Name = dto.Name,
                    IsMain = dto.IsMain,
                    Icon = iconPath
                });
            }

            this._context.Resources.AddRange(savedResources);
            await this._context.SaveChangesAsync();

            var response = savedResources.Select(r => new ResourceDto
            {
                Id = (int)r.Id,
                Name = r.Name,
                IsMain = r.IsMain,
                Icon = r.Icon != null ? $"{this.Request.Scheme}://{this.Request.Host}/{r.Icon}" : null,
            }).ToList();

            return this.CreatedAtAction(nameof(this.PostResources), null, response);
        }


        // DELETE: api/Resources
        [HttpDelete]
        public async Task<ActionResult> DeleteResources([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var resources = await _context.Resources.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (resources.Count == 0)
            {
                return this.NotFound("Nie znaleziono zasobów do usunięcia.");
            }

            this._context.Resources.RemoveRange(resources);
            await this._context.SaveChangesAsync();

            return this.Ok();
        }


        [HttpGet("nation/resource-balance/{nationId?}")]
        public async Task<ActionResult<NationResourceBalanceDto>> GetNationResourceBalance(int? nationId)
        {
            nationId ??= _nationId;
            var result = await CalcResourceBalance.CalculateNationResourceBalance(nationId.Value, _context);
            if (result == null)
                return NotFound();
            return Ok(result);
        }





        [HttpGet("nation/owned-resources/{nationId?}")]
        public async Task<ActionResult<List<ResourceAmountDto>>> GetOwnedResources(int? nationId)
        {

            if (nationId == null)
            {
                nationId = this._nationId;
            }

            if (nationId == null)
            {
                return this.BadRequest("Brak ID państwa.");
            }

            var query = this._context.OwnedResources
                .AsNoTracking()
                .Include(or => or.Resource)
                .Where(or => or.NationId == nationId);


            var ownedList = await query.ToListAsync();

            if (ownedList == null || ownedList.Count == 0)
            {
                return this.NotFound($"Nie znaleziono zasobów przypisanych do państwa o ID: {nationId}");
            }

            var ownedResources = await _context.OwnedResources
                .Where(or => or.NationId == nationId)
                .GroupBy(or => new { or.ResourceId, ResourceName = or.Resource.Name })
                .Select(g => new ResourceAmountDto
                {
                    ResourceId = g.Key.ResourceId,
                    ResourceName = g.Key.ResourceName,
                    Amount = g.Sum(x => x.Amount) 
                })
                .ToListAsync();

            return this.Ok(ownedResources);
        }


        [HttpPut("nation/owned-resources/{nationId?}")]
        public async Task<IActionResult> PutOwnedResources(int? nationId, [FromBody] List<ResourceAmountDto> resources)
        {
            // Ustal nationId domyślnie
            if (nationId == null)
            {
                nationId = this._nationId;
            }

            if (nationId == null)
            {
                return this.BadRequest("Brak ID państwa.");
            }

            if (resources == null || !resources.Any())
            {
                return this.BadRequest("Brak danych do zapisania.");
            }


            using var transaction = await this._context.Database.BeginTransactionAsync();
            try
            {
                var existingOwned = await this._context.OwnedResources
                    .Where(or => or.NationId == nationId)
                    .ToListAsync();

                foreach (var dto in resources)
                {

                    var match = existingOwned.FirstOrDefault(e => e.ResourceId == dto.ResourceId);
                    if (match != null)
                    {
                        if (Math.Abs(match.Amount - dto.Amount) > float.Epsilon)
                        {
                            match.Amount = dto.Amount;
                            this._context.Entry(match).State = EntityState.Modified;
                        }
                    }
                }


                await this._context.SaveChangesAsync();
                await transaction.CommitAsync();

                return this.NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return this.StatusCode(500, $"Błąd podczas aktualizacji zasobów: {ex.Message}");
            }
        }




    }
}
