namespace Wg_backend_api.Controllers.GameControllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Wg_backend_api.Auth;
    using Wg_backend_api.Data;
    using Wg_backend_api.DTO;
    using Wg_backend_api.Models;
    using Wg_backend_api.Services;

    [Route("api/Factions")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class FactionsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        private int? _nationId;


        public FactionsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        public async Task<ActionResult<IEnumerable<FactionDTO>>> GetFactions(int? id)
        {
            if (id.HasValue)
            {
                var faction = await this._context.Factions
                    .Where(f => f.Id == id)
                    .Select(f => new FactionDTO
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Power = f.Power,
                        Agenda = f.Agenda,
                        Contentment = f.Contentment,
                        Color = f.Color,
                        Description = f.Description,
                        NationId = f.NationId,

                    })
                    .FirstOrDefaultAsync();

                if (faction == null)
                {
                    return this.NotFound();
                }

                return this.Ok(new List<FactionDTO> { faction });
            }
            else
            {
                var factions = await this._context.Factions
                    .Select(f => new FactionDTO
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Power = f.Power,
                        Agenda = f.Agenda,
                        Contentment = f.Contentment,
                        Color = f.Color,
                        Description = f.Description,
                        NationId = f.NationId,
                    })
                    .ToListAsync();

                return this.Ok(factions);
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutFactions([FromBody] List<FactionDTO> factionsDto)
        {
            if (factionsDto == null || factionsDto.Count == 0)
            {
                return this.BadRequest("Brak danych do edycji.");
            }

            foreach (var dto in factionsDto)
            {
                var faction = await _context.Factions.FindAsync(dto.Id);
                if (faction == null)
                {
                    return this.NotFound($"Nie znaleziono frakcji o ID {dto.Id}.");
                }

                faction.Name = dto.Name;
                faction.Power = dto.Power;
                faction.Agenda = dto.Agenda;
                faction.Contentment = dto.Contentment;
                faction.Color = dto.Color;
                faction.Description = dto.Description;
                faction.NationId = (int)dto.NationId;

                this._context.Entry(faction).State = EntityState.Modified;
            }

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return this.StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return this.NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<FactionDTO>> PostFactions([FromBody] List<FactionDTO> factionsDto)
        {
            if (factionsDto == null || factionsDto.Count == 0)
            {
                return this.BadRequest("Brak danych do zapisania.");
            }

            var factions = factionsDto.Select(dto => new Faction
            {
                Name = dto.Name,
                Power = dto.Power,
                Agenda = dto.Agenda,
                Contentment = dto.Contentment,
                Color = dto.Color,
                Description = dto.Description,
                NationId = dto.NationId > 0 ? (int)dto.NationId : _nationId ?? throw new InvalidOperationException("Brak ID państwa w sesji."),
            }).ToList();

            this._context.Factions.AddRange(factions);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction("GetFactions", new { id = factions[0].Id }, factionsDto);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteFactions([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var factions = await this._context.Factions.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (factions.Count == 0)
            {
                return this.NotFound("Nie znaleziono frakcji do usunięcia.");
            }

            this._context.Factions.RemoveRange(factions);
            await this._context.SaveChangesAsync();

            return this.Ok();
        }

        [HttpGet("ByNation/{nationId?}")]
        public async Task<ActionResult<IEnumerable<FactionDTO>>> GetFactionsByNation(int? nationId)
        {
            if (nationId == null)
            {
                nationId = this._nationId;
            }

            var factions = await this._context.Factions
                .Where(f => f.NationId == nationId)
                .Select(f => new FactionDTO
                {
                    Id = f.Id,
                    Name = f.Name,
                    Power = f.Power,
                    Agenda = f.Agenda,
                    Contentment = f.Contentment,
                    Color = f.Color,
                    Description = f.Description,
                    NationId = f.NationId,
                })
                .ToListAsync();

            if (factions == null || factions.Count == 0)
            {
                return this.NotFound($"Nie znaleziono frakcji dla państwa o ID {nationId}.");
            }

            return this.Ok(factions);
        }
    }
}
