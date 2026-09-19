using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class CulturesController : GameControllerBase
    {
        public CulturesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        // GET: api/Cultures
        // GET: api/Cultures/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<CultureDTO>>> GetCultures(int? id)
        {
            if (id.HasValue)
            {
                var culture = await this.Context.Cultures.FindAsync(id);
                if (culture == null)
                {
                    return this.NotFound();
                }

                return this.Ok(new List<CultureDTO> { new CultureDTO { Id = culture.Id, Name = culture.Name } });
            }
            else
            {
                var cultures = await this.Context.Cultures.ToListAsync();
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
                var culture = await this.Context.Cultures.FindAsync(cultureDTO.Id);
                if (culture == null)
                {
                    return this.NotFound($"Nie znaleziono kultury o ID {cultureDTO.Id}.");
                }

                culture.Name = cultureDTO.Name;
                this.Context.Entry(culture).State = EntityState.Modified;
            }

            try
            {
                await this.Context.SaveChangesAsync();
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
            this.Context.Cultures.AddRange(cultures);
            await this.Context.SaveChangesAsync();

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

            var cultures = await this.Context.Cultures.Where(c => ids.Contains(c.Id)).ToListAsync();

            if (cultures.Count == 0)
            {
                return this.NotFound("Nie znaleziono kultur do usunięcia.");
            }

            this.Context.Cultures.RemoveRange(cultures);
            await this.Context.SaveChangesAsync();

            return this.Ok();
        }
    }
}
