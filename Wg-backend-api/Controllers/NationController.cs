﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers
{
    [Route("api/Nations")]
    [ApiController]
    public class NationController : Controller
    {
        private readonly AppDbContext _context;

        public NationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Religions
        // GET: api/Religions/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<Nation>>> GetNations(int? id)
        {
            if (id.HasValue)
            {
                var nation = await _context.Nations.FindAsync(id);
                if (nation == null)
                {
                    return NotFound();
                }
                return Ok(new List<Nation> { nation });
            }
            else
            {
                return await _context.Nations.ToListAsync();
            }
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Nation>>> GetNations()
        {  
           return await _context.Nations.ToListAsync();
            
        }

        // PUT: api/Religions
        [HttpPut]
        public async Task<IActionResult> PutNations([FromBody] List<Nation> nations)
        {
            if (nations == null || nations.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var nation in nations)
            {
                _context.Entry(nation).State = EntityState.Modified;
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

        // POST: api/Religions
        [HttpPost]
        public async Task<ActionResult<Nation>> PostNations([FromBody] List<Nation> nations)
        {
            if (nations == null || nations.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (Nation nation in nations)
            {
                nation.Id = null;
            }

            _context.Nations.AddRange(nations);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNations", new { id = nations[0].Id }, nations);
        }

        // DELETE: api/Religions
        [HttpDelete]
        public async Task<ActionResult> DeleteNations([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var nations = await _context.Nations.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (nations.Count == 0)
            {
                return NotFound("Nie znaleziono panstwa do usunięcia.");
            }

            _context.Nations.RemoveRange(nations);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
