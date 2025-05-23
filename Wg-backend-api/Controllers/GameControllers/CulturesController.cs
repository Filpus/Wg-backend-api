﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CulturesController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public CulturesController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;

            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }
            _context = _gameDbContextFactory.Create(schema);
        }



        // GET: api/Cultures
        // GET: api/Cultures/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Culture>>> GetCultures(int? id)
        {
            if (id.HasValue)
            {
                var culture = await _context.Cultures.FindAsync(id);
                if (culture == null)
                {
                    return NotFound();
                }
                return Ok(new List<Culture> { culture });
            }
            else
            {
                return await _context.Cultures.ToListAsync();
            }
        }

        // PUT: api/Cultures
        [HttpPut]
        public async Task<IActionResult> PutCultures([FromBody] List<Culture> cultures)
        {
            if (cultures == null || cultures.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var culture in cultures)
            {
                _context.Entry(culture).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        // POST: api/Cultures
        [HttpPost]
        public async Task<ActionResult<Culture>> PostCultures([FromBody] List<Culture> cultures)
        {
            if (cultures == null || cultures.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }


            foreach (Culture culture in cultures)
            {
                culture.Id = null;
            }

            _context.Cultures.Add(cultures[0]);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCultures", new { id = cultures[0].Id }, cultures);
        }

        // DELETE: api/Cultures
        [HttpDelete]
        public async Task<ActionResult> DeleteCultures([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var cultures = await _context.Cultures.Where(c => ids.Contains(c.Id)).ToListAsync();

            if (cultures.Count == 0)
            {
                return NotFound("Nie znaleziono kultur do usunięcia.");
            }

            _context.Cultures.RemoveRange(cultures);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
