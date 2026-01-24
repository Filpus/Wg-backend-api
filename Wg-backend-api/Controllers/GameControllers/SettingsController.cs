using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/ArmySettings")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class ArmySettingsController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public ArmySettingsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<ArmySettings>>> GetArmySettings()
        {
            ArmySettings settings = ArmySettings.GetRowAsync(this._context).Result;
            return this.Ok(new List<ArmySettings> { settings });
        }

        // PUT: api/ArmySettings
        [HttpPut]
        public async Task<IActionResult> PutArmySettings([FromBody] List<ArmySettings> settings)
        {
            if (settings == null || settings.Count == 0)
            {
                return this.BadRequest("Brak danych do edycji.");
            }

            foreach (var s in settings)
            {
                if (s.Id == null || s.Id <= 0)
                {
                    return this.BadRequest("Brak lub nieprawid³owe ID ustawienia do edycji.");
                }

                if (string.IsNullOrWhiteSpace(s.NameOfSettingsSet) || s.NameOfSettingsSet.Length > 50)
                {
                    return this.BadRequest("Nazwa zestawu ustawieñ jest niepoprawnej d³ugoœci.");
                }
            }

            foreach (var s in settings)
            {
                var entity = await this._context.ArmySettings.FindAsync(s.Id.Value);
                if (entity == null)
                {
                    return this.NotFound($"Ustawienie o ID {s.Id} nie istnieje.");
                }

                // Map wszystkie pola z przes³anego obiektu na encjê
                entity.NameOfSettingsSet = s.NameOfSettingsSet;
                entity.UseMeleeAtack = s.UseMeleeAtack;
                entity.UseRangeAtack = s.UseRangeAtack;
                entity.UseDefense = s.UseDefense;
                entity.UseSpeed = s.UseSpeed;
                entity.UseMorale = s.UseMorale;
                entity.UseMaintanace = s.UseMaintanace;

                this._context.Entry(entity).State = EntityState.Modified;
            }

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return this.StatusCode(500, "B³¹d podczas aktualizacji.");
            }

            return this.NoContent();
        }

        // POST: api/ArmySettings
        [HttpPost]
        public async Task<ActionResult<IEnumerable<ArmySettings>>> PostArmySettings([FromBody] List<ArmySettings> settings)
        {
            if (settings == null || settings.Count == 0)
            {
                return this.BadRequest("Brak danych do zapisania.");
            }

            var entities = new List<ArmySettings>();

            foreach (var s in settings)
            {
                if (string.IsNullOrWhiteSpace(s.NameOfSettingsSet) || s.NameOfSettingsSet.Length > 50)
                {
                    return this.BadRequest("Nazwa zestawu ustawieñ jest niepoprawnej d³ugoœci.");
                }

                var entity = new ArmySettings
                {
                    NameOfSettingsSet = s.NameOfSettingsSet,
                    UseMeleeAtack = s.UseMeleeAtack,
                    UseRangeAtack = s.UseRangeAtack,
                    UseDefense = s.UseDefense,
                    UseSpeed = s.UseSpeed,
                    UseMorale = s.UseMorale,
                    UseMaintanace = s.UseMaintanace
                };

                entities.Add(entity);
            }

            this._context.ArmySettings.AddRange(entities);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction("GetArmySettings", new { id = entities.First().Id }, entities);
        }

        // DELETE: api/ArmySettings
        [HttpDelete]
        public async Task<ActionResult> DeleteArmySettings([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequest("Brak ID do usuniêcia.");
            }

            var settings = await this._context.ArmySettings.Where(s => ids.Contains(s.Id)).ToListAsync();

            if (settings.Count == 0)
            {
                return this.NotFound("Nie znaleziono ustawieñ do usuniêcia.");
            }

            this._context.ArmySettings.RemoveRange(settings);
            await this._context.SaveChangesAsync();

            return this.Ok();
        }
    }
}
