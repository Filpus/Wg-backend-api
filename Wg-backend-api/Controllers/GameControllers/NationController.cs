namespace Wg_backend_api.Controllers.GameControllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Wg_backend_api.Data;
    using Wg_backend_api.DTO;
    using Wg_backend_api.Models;
    using Wg_backend_api.Services;

    [Route("api/Nations")]
    [ApiController]
    public class NationController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private GlobalDbContext _globalContext;

        public NationController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, GlobalDbContext globalDbContext)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;
            this._globalContext = globalDbContext;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = this._gameDbContextFactory.Create(schema);
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<NationDTO>>> GetNations(int? id)
        {
            if (id.HasValue)
            {
                var nation = await this._context.Nations.FindAsync(id);
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
                var nations = await this._context.Nations.ToListAsync();
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

        [HttpGet("other-nations")]
        public async Task<List<NationBaseInfoDTO>> GetOtherNations()
        {
            var nationId = this._sessionDataService.GetNation();

            if (string.IsNullOrEmpty(nationId))
            {
                return new List<NationBaseInfoDTO>();
            }

            int id = int.Parse(nationId);

            return await this._context.Nations
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

            var nationsWithUsersId = await this._context.Nations
                .Join(_context.Assignments,
                    n => n.Id,
                    a => a.NationId,
                    (n, a) => new { Id = n.Id, Name = n.Name, Flag = n.Flag, Color = n.Color, OwnerId = a.UserId})
                .Join(_context.Players,
                    r => r.OwnerId,
                    p => p.Id,
                    (r, p) => new
                    {
                        Id = r.Id!.Value,
                        Name = r.Name,
                        Flag = r.Flag,
                        Color = r.Color,
                        OwnerId = p.UserId,
                    })
                .ToListAsync();

            var players = await this._globalContext.Users.Select(u => new {u.Id, u.Name}).ToListAsync();

            var nationsWithUsers = nationsWithUsersId.Select(n => new NationWithOwnerDTO
            {
                Id = n.Id,
                Name = n.Name,
                Flag = n.Flag,
                Color = n.Color,
                OwnerName = players.FirstOrDefault(p => p.Id == n.OwnerId)?.Name,
            }).ToList();

            return nationsWithUsers;
        }

        [HttpPut]
        public async Task<IActionResult> PutNations([FromBody] List<NationDTO> nations)
        {
            if (nations == null || nations.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var nationDto in nations)
            {
                var nation = await this._context.Nations.FindAsync(nationDto.Id);
                if (nation == null)
                {
                    return NotFound($"Nie znaleziono państwa o ID {nationDto.Id}.");
                }

                nation.Name = nationDto.Name;
                nation.ReligionId = nationDto.ReligionId;
                nation.CultureId = nationDto.CultureId;

                this._context.Entry(nation).State = EntityState.Modified;
            }

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<NationDTO>> PostNations([FromBody] List<NationDTO> nations)
        {
            if (nations == null || nations.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var newNations = nations.Select(nationDto => new Nation
            {
                Name = nationDto.Name,
                ReligionId = nationDto.ReligionId,
                CultureId = nationDto.CultureId,
            }).ToList();

            this._context.Nations.AddRange(newNations);
            await this._context.SaveChangesAsync();

            return CreatedAtAction("GetNations", new { id = newNations[0].Id }, newNations.Select(n => new NationDTO
            {
                Id = n.Id,
                Name = n.Name,
                ReligionId = n.ReligionId,
                CultureId = n.CultureId,
                Color = n.Color,
            }));
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteNations([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var nations = await this._context.Nations.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (nations.Count == 0)
            {
                return this.NotFound("Nie znaleziono państwa do usunięcia.");
            }

            foreach (var nation in nations)
            {
                if (nation.Id.HasValue && this.IsNationDependency(nation.Id.Value))
                {
                    return this.BadRequest($"Can't delete nation {nation.Id}, because nation is dependency");
                }
            }

            this._context.Nations.RemoveRange(nations);
            await this._context.SaveChangesAsync();

            return this.Ok();
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
