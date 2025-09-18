namespace Wg_backend_api.Controllers.GameControllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Wg_backend_api.Data;
    using Wg_backend_api.DTO;
    using Wg_backend_api.Models;
    using Wg_backend_api.Services;

    [Route("api/[controller]")]
    [ApiController]
    public class CulturesController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public CulturesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;

            string schema = this._sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this._context = this._gameDbContextFactory.Create(schema);
        }

        // GET: api/Cultures
        // GET: api/Cultures/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<CultureDTO>>> GetCultures(int? id)
        {
            if (id.HasValue)
            {
                var culture = await this._context.Cultures.FindAsync(id);
                if (culture == null)
                {
                    return this.NotFound();
                }

                return this.Ok(new List<CultureDTO> { new CultureDTO { Id = culture.Id, Name = culture.Name } });
            }
            else
            {
                var cultures = await this._context.Cultures.ToListAsync();
                return this.Ok(cultures.Select(c => new CultureDTO { Id = c.Id, Name = c.Name }));
            }
        }

        // PUT: api/Cultures
        [HttpPut]
        public async Task<IActionResult> PutCultures([FromBody] List<CultureDTO> cultureDTOs)
        {
            if (cultureDTOs == null || cultureDTOs.Count == 0)
            {
                return this.BadRequest("Brak danych do edycji.");
            }

            foreach (var cultureDTO in cultureDTOs)
            {
                if (string.IsNullOrWhiteSpace(cultureDTO.Name) || cultureDTO.Name.Length > 64)
                {
                    return this.BadRequest("Nazwa kultury jest niepoprawnej długości.");
                }
            }

            foreach (var cultureDTO in cultureDTOs)
            {
                var culture = await this._context.Cultures.FindAsync(cultureDTO.Id);
                if (culture == null)
                {
                    return this.NotFound($"Nie znaleziono kultury o ID {cultureDTO.Id}.");
                }

                culture.Name = cultureDTO.Name;
                this._context.Entry(culture).State = EntityState.Modified;
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

        // POST: api/Cultures
        [HttpPost]
        public async Task<ActionResult<CultureDTO>> PostCultures([FromBody] List<CultureDTO> cultureDTOs)
        {
            if (cultureDTOs == null || cultureDTOs.Count == 0)
            {
                return this.BadRequest("Brak danych do zapisania.");
            }

            foreach (var cultureDTO in cultureDTOs)
            {
                if (string.IsNullOrWhiteSpace(cultureDTO.Name) || cultureDTO.Name.Length > 64)
                {
                    return this.BadRequest("Nazwa kultury jest niepoprawnej długości.");
                }
            }

            var cultures = cultureDTOs.Select(dto => new Culture { Name = dto.Name }).ToList();
            this._context.Cultures.AddRange(cultures);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction("GetCultures", new { id = cultures[0].Id }, cultureDTOs);
        }

        // DELETE: api/Cultures
        [HttpDelete]
        public async Task<ActionResult> DeleteCultures([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var cultures = await this._context.Cultures.Where(c => ids.Contains(c.Id)).ToListAsync();

            if (cultures.Count == 0)
            {
                return this.NotFound("Nie znaleziono kultur do usunięcia.");
            }

            this._context.Cultures.RemoveRange(cultures);
            await this._context.SaveChangesAsync();

            return this.Ok();
        }
    }
}
