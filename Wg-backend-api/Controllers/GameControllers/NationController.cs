using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Nations")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class NationController : GameControllerBase
    {
        private readonly GlobalDbContext _globalDbContext;

        private class FileUploadResult
        {
            public bool Success { get; set; }

            public string? FilePath { get; set; }

            public string? ErrorMessage { get; set; }
        }

        public NationController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, GlobalDbContext globalDbContext)
            : base(gameDbFactory, sessionDataService)
        {
            this._globalDbContext = globalDbContext;
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<NationDTO>>> GetNations(int? id)
        {
            if (id.HasValue)
            {
                var nation = await this.Context.Nations.FindAsync(id);
                if (nation == null)
                {
                    return NotFound();
                }

                return Ok(new List<NationDTO>
                {
                    new NationDTO
                    {
                        Id = nation.Id,
                        Name = nation.Name,
                        ReligionId = nation.ReligionId,
                        CultureId = nation.CultureId,
                        Color = nation.Color,
                    },
                });
            }
            else
            {
                var nations = await this.Context.Nations.ToListAsync();
                return Ok(nations.Select(n => new NationDTO
                {
                    Id = n.Id,
                    Name = n.Name,
                    ReligionId = n.ReligionId,
                    CultureId = n.CultureId,
                    Color = n.Color,
                }));
            }
        }

        [HttpGet("no-owner")]
        public async Task<List<NationBaseInfoDTO>> GetNationsWithNoOwner()
        {
            var nations = await this.Context.Nations
                .Select(na => new
                {
                    na.Id,
                    na.Name,
                    Assigment = na.Assignment,
                })
                .Where(na => na.Assigment == null)
                .Select(na => new NationBaseInfoDTO
                {
                    Id = na.Id,
                    Name = na.Name,
                })
                .ToListAsync();

            return nations;
        }

        [HttpGet("other-nations")]
        public async Task<List<NationBaseInfoDTO>> GetOtherNations()
        {
            var nationId = this.SessionDataService.GetNation();

            if (string.IsNullOrEmpty(nationId))
            {
                return [];
            }

            int id = int.Parse(nationId);

            return await this.Context.Nations
                .Where(n => n.Id != id)
                .Select(n => new NationBaseInfoDTO
                {
                    Id = n.Id,
                    Name = n.Name,
                })
                .ToListAsync();
        }

        [HttpGet("with-owner")]
        public async Task<List<NationWithOwnerDTO>> GetNationsWithOwners()
        {
            // TODO ensure only mg can call this endpoint

            var nationsWithUsers = await this.Context.Nations
                .GroupJoin(
                    this.Context.Assignments.Where(a => a.IsActive),
                    n => n.Id,
                    a => a.NationId,
                    (n, assignments) => new { n, assignments }
                )
                .SelectMany(
                    x => x.assignments.DefaultIfEmpty(),
                    (x, a) => new { x.n, a }
                )
                .GroupJoin(
                    this.Context.Players,
                    na => na.a.UserId,
                    p => p.Id,
                    (na, players) => new { na, players }
                )
                .SelectMany(
                    x => x.players.DefaultIfEmpty(),
                    (x, p) => new NationWithOwnerDTO
                    {
                        Id = x.na.n.Id,
                        Name = x.na.n.Name,
                        Flag = x.na.n.Flag,
                        Color = x.na.n.Color,
                        OwnerName = p != null ? p.Name : null
                    }
                )
                .ToListAsync();

            return nationsWithUsers;
        }

        [HttpGet("detailed-nation/{id?}")]
        public async Task<ActionResult<NationDetailedDTO>> GetNationDetailedDTO(int? id)
        {
            int nationId;

            if (id == null)
            {
                var nationIdStr = this.SessionDataService.GetNation();

                if (string.IsNullOrEmpty(nationIdStr))
                {
                    return this.BadRequest("No nation in session");
                }

                nationId = int.Parse(nationIdStr);
            }
            else
            {
                nationId = id.Value;
            }

            var nation = await this.Context.Nations
                .Where(n => n.Id == nationId)
                .Select(n => new NationDetailedDTO
                {
                    Id = n.Id!.Value,
                    Name = n.Name,
                    Flag = n.Flag,
                    Color = n.Color,

                    Culture = new CultureDTO
                    {
                        Id = n.Culture.Id,
                        Name = n.Culture.Name,
                    },

                    Religion = new ReligionDTO
                    {
                        Id = n.Religion.Id,
                        Name = n.Religion.Name,
                    },

                    OwnerName = this.Context.Assignments
                        .Where(a => a.NationId == n.Id && a.IsActive)
                        .Join(
                            this.Context.Players,
                            a => a.UserId,
                            p => p.Id,
                            (a, p) => p.Name)
                        .FirstOrDefault() ?? null,
                })
                .FirstOrDefaultAsync();

            if (nation == null)
            {
                return this.NotFound();
            }

            return this.Ok(nation);
        }

        [HttpPut]
        public async Task<IActionResult> PutNations([FromBody] List<NationDTO> nations)
        {
            var selectedGame = this.SessionDataService.GetSchema();
            if (string.IsNullOrEmpty(selectedGame) || !selectedGame.StartsWith("game_"))
            {
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = "No game selected in session",
                });
            }

            var gameId = int.Parse(selectedGame.Split('_')[1]);

            var globalGameAccesse = this._globalDbContext.GameAccesses.Where(ga => ga.GameId == gameId).ToList();

            if (nations == null || nations.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var nationDto in nations)
            {
                var nation = await this.Context.Nations.FindAsync(nationDto.Id);
                if (nation == null)
                {
                    return NotFound($"Nie znaleziono państwa o ID {nationDto.Id}.");
                }

                if (nation.Name != nationDto.Name)
                {
                    nation.Name = nationDto.Name;
                    var accessesToUpdate = globalGameAccesse.Where(ga => ga.NationName == nation.Name).ToList();
                    foreach (var access in accessesToUpdate)
                    {
                        access.NationName = nationDto.Name;
                        this._globalDbContext.Entry(access).State = EntityState.Modified;
                    }
                }

                nation.ReligionId = nationDto.ReligionId;
                nation.CultureId = nationDto.CultureId;

                this.Context.Entry(nation).State = EntityState.Modified;
            }

            try
            {
                await this.Context.SaveChangesAsync();
                await this._globalDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<NationDTO>> PostNations([FromForm] NationCreateDTO nations)
        {
            if (nations == null)
            {
                return this.BadRequest("Brak danych do zapisania.");
            }

            if (nations.ReligionId == null || nations.CultureId == null || nations.Color == null || nations.Name == null)
            {
                return this.BadRequest("Religion, Culture, Color are required.");
            }

            var nationWithSameName = await this.Context.Nations
                .FirstOrDefaultAsync(n => n.Name.ToLower() == nations.Name.ToLower());

            if (nationWithSameName != null)
            {
                return this.BadRequest("Państwo o takiej nazwie już istnieje.");
            }

            string? flagPath = null;

            if (nations.Flag is not null)
            {
                var uploadResult = await this.UploadFile(nations.Flag);
                if (!uploadResult.Success)
                {
                    return this.BadRequest(uploadResult.ErrorMessage);
                }

                flagPath = uploadResult.FilePath;
            }

            var newNation = new Nation
            {
                Name = nations.Name,
                ReligionId = (int)nations.ReligionId,
                CultureId = (int)nations.CultureId,
                Color = nations.Color,
                Flag = flagPath,
            };

            this.Context.Nations.Add(newNation);
            await this.Context.SaveChangesAsync();

            return this.CreatedAtAction(nameof(GetNations), new { id = newNation.Id }, new NationDTO
            {
                Id = newNation.Id,
                Name = newNation.Name,
                ReligionId = newNation.ReligionId,
                CultureId = newNation.CultureId,
                Color = newNation.Color,
                Flag = newNation.Flag,
            });
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteNations([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var nations = await this.Context.Nations.Where(r => ids.Contains(r.Id)).ToListAsync();
            var selectedGame = this.SessionDataService.GetSchema();
            if (string.IsNullOrEmpty(selectedGame) || !selectedGame.StartsWith("game_"))
            {
                return BadRequest(new
                {
                    error = "Bad Request",
                    message = "No game selected in session",
                });
            }

            var gameId = int.Parse(selectedGame.Split('_')[1]);
            var globalGameAccesse = this._globalDbContext.GameAccesses.Where(ga => ga.GameId == gameId).ToList();

            if (nations.Count == 0)
            {
                return this.NotFound("Nie znaleziono państwa do usunięcia.");
            }

            foreach (var nation in nations)
            {
                globalGameAccesse
                    .Where(ga => ga.NationName == nation.Name)
                    .ToList()
                    .ForEach(ga =>
                    {
                        ga.NationName = null;
                        this._globalDbContext.Entry(ga).State = EntityState.Modified;
                    });
                if (nation.Id.HasValue && this.IsNationDependency(nation.Id.Value))
                {
                    return this.BadRequest($"Can't delete nation {nation.Id}, because nation is dependency");
                }
            }

            // Remember here docks and barracks in armies with location null will be deleted because of cascade delete
            this._globalDbContext.GameAccesses.UpdateRange(globalGameAccesse);
            await this._globalDbContext.SaveChangesAsync();
            this.Context.Nations.RemoveRange(nations);
            await this.Context.SaveChangesAsync();

            return this.Ok();
        }

        [HttpPatch]
        public async Task<IActionResult> PatchNations([FromForm] NationCreateDTO nationDto)
        {
            if (nationDto == null)
            {
                return this.BadRequest("No data to edit.");
            }

            if (nationDto.Id == null)
            {
                return this.BadRequest("Nation ID is required.");
            }

            var nation = await this.Context.Nations.FindAsync(nationDto.Id);
            if (nation == null)
            {
                return this.NotFound($"Didn't find nation with ID {nationDto.Id}.");
            }

            if (nationDto.Name != null)
            {
                var nationWithSameName = await this.Context.Nations
                    .FirstOrDefaultAsync(n => n.Name.ToLower() == nationDto.Name.ToLower() && n.Id != nationDto.Id);
                if (nationWithSameName != null)
                {
                    return this.BadRequest("Nation with the same name already exists.");
                }

                var selectedGame = this.SessionDataService.GetSchema();
                if (string.IsNullOrEmpty(selectedGame) || !selectedGame.StartsWith("game_"))
                {
                    return BadRequest(new
                    {
                        error = "Bad Request",
                        message = "No game selected in session",
                    });
                }

                var gameId = int.Parse(selectedGame.Split('_')[1]);

                var globalGameAccesse = this._globalDbContext.GameAccesses.Where(ga => ga.GameId == gameId && ga.NationName == nation.Name).ToList();
                foreach (var access in globalGameAccesse)
                {
                    access.NationName = nationDto.Name;
                    this._globalDbContext.Entry(access).State = EntityState.Modified;
                }

                nation.Name = nationDto.Name;
            }

            if (nationDto.ReligionId != null)
            {
                var religion = await this.Context.Religions.FindAsync(nationDto.ReligionId);
                if (religion == null)
                {
                    return this.NotFound($"Didn't find Religion with ID {nationDto.ReligionId}.");
                }

                nation.Religion = religion;
            }

            if (nationDto.CultureId != null)
            {
                var culture = await this.Context.Cultures.FindAsync(nationDto.CultureId);
                if (culture == null)
                {
                    return this.NotFound($"Didn't find culture with ID {nationDto.CultureId}.");
                }

                nation.Culture = culture;
            }

            if (nationDto.Color != null)
            {
                if (!this.IsProperColor(nationDto.Color))
                {
                    return this.BadRequest("Color is not proper");
                }

                nation.Color = nationDto.Color;
            }

            string? flagPath = null;

            if (nationDto.Flag != null)
            {
                var result = await this.UploadFile(nationDto.Flag);
                if (!result.Success)
                {
                    return this.BadRequest(result.ErrorMessage);
                }

                nation.Flag = result.FilePath;
            }

            this.Context.Entry(nation).State = EntityState.Modified;

            try
            {
                await this.Context.SaveChangesAsync();
                await this._globalDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                try
                {
                    if (System.IO.File.Exists(flagPath))
                    {
                        System.IO.File.Delete(flagPath);
                    }
                }
                catch
                {
                    // TODO log error
                }

                return this.StatusCode(500, "Error updating nations {ex.Message}");
            }

            return this.NoContent();
        }

        private async Task<FileUploadResult> UploadFile(IFormFile file)
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

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // $"{Request.Scheme}://{Request.Host}{imageUrl}",
            var pathFile = $"/images/{uniqueFileName}";

            return new FileUploadResult { Success = true, FilePath = pathFile };
        }

        private bool IsNationDependency(int id)
        {
            var hasDependencies = this.Context.AccessToUnits.Any(e => e.NationId == id) ||
                                  this.Context.Actions.Any(e => e.NationId == id) ||
                                  this.Context.Armies.Any(e => e.NationId == id && e.LocationId != null) ||
                                  this.Context.Factions.Any(e => e.NationId == id) ||
                                  this.Context.Localisations.Any(e => e.NationId == id) ||
                                  this.Context.RelatedEvents.Any(e => e.NationId == id) ||
                                  this.Context.TradeAgreements.Any(e => e.OfferingNationId == id || e.ReceivingNationId == id) ||
                                  this.Context.UnitOrders.Any(e => e.NationId == id);

            return hasDependencies;
        }

        private bool IsProperColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return false;
            }

            var hexPattern = @"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$";
            if (Regex.IsMatch(color, hexPattern))
            {
                return true;
            }

            return false;
        }
    }
}
