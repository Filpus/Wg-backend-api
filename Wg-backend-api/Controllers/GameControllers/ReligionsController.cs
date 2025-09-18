namespace Wg_backend_api.Controllers.GameControllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Resources;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Wg_backend_api.Data;
    using Wg_backend_api.DTO;
    using Wg_backend_api.Models;
    using Wg_backend_api.Services;

    [Route("api/Religions")]
    [ApiController]
    public class ReligionsControler : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public ReligionsControler(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        // GET: api/Religions
        // GET: api/Religions/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<ReligionDTO>>> GetReligions(int? id)
        {
            if (id.HasValue)
            {
                var religion = await this._context.Religions.FindAsync(id);
                if (religion == null)
                {
                    return this.NotFound();
                }

                return this.Ok(new List<ReligionDTO>
                {
                    new ReligionDTO { Id = religion.Id, Name = religion.Name },
                });
            }
            else
            {
                var religions = await this._context.Religions.ToListAsync();
                return this.Ok(religions.Select(r => new ReligionDTO { Id = r.Id, Name = r.Name }));
            }
        }

        // PUT: api/Religions
        [HttpPut]
        public async Task<IActionResult> PutReligions([FromBody] List<ReligionDTO> religionDTOs)
        {
            if (religionDTOs == null || religionDTOs.Count == 0)
            {
                return this.BadRequest("Brak danych do edycji.");
            }

            foreach (var religionDTO in religionDTOs)
            {
                if (string.IsNullOrWhiteSpace(religionDTO.Name) || religionDTO.Name.Length > 25)
                {
                    return this.BadRequest("Nazwa religii jest niepoprawnej długości.");
                }
            }

            foreach (var religionDTO in religionDTOs)
            {
                var religion = new Religion { Id = religionDTO.Id, Name = religionDTO.Name };
                this._context.Entry(religion).State = EntityState.Modified;
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

        // POST: api/Religions
        [HttpPost]
        public async Task<ActionResult<IEnumerable<ReligionDTO>>> PostReligions([FromBody] List<ReligionDTO> religionDTOs)
        {
            if (religionDTOs == null || religionDTOs.Count == 0)
            {
                return this.BadRequest("Brak danych do zapisania.");
            }

            var religions = new List<Religion>();

            foreach (var religionDTO in religionDTOs)
            {
                if (string.IsNullOrWhiteSpace(religionDTO.Name) || religionDTO.Name.Length > 25)
                {
                    return this.BadRequest("Nazwa religii jest niepoprawnej długości.");
                }
            }

            foreach (var religionDTO in religionDTOs)
            {
                if (string.IsNullOrWhiteSpace(religionDTO.Name))
                {
                    return this.BadRequest("Brak nazwy religii.");
                }

                religions.Add(new Religion { Name = religionDTO.Name });
            }

            this._context.Religions.AddRange(religions);
            await this._context.SaveChangesAsync();

            var createdDTOs = religions.Select(r => new ReligionDTO { Id = r.Id, Name = r.Name }).ToList();
            return this.CreatedAtAction("GetReligions", new { id = createdDTOs[0].Id }, createdDTOs);
        }

        // DELETE: api/Religions
        [HttpDelete]
        public async Task<ActionResult> DeleteReligions([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usunięcia.");
            }

            var religions = await this._context.Religions.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (religions.Count == 0)
            {
                return this.NotFound("Nie znaleziono religii do usunięcia.");
            }

            this._context.Religions.RemoveRange(religions);
            await this._context.SaveChangesAsync();

            return this.Ok();
        }
    }
}
