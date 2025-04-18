﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PopulationsController : ControllerBase
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private GameDbContext _context;
        public PopulationsController(IGameDbContextFactory gameDbFactory)
        {
            _gameDbContextFactory = gameDbFactory;

            string schema = HttpContext.Session.GetString("Schema");
            _context = _gameDbContextFactory.Create(schema);
        }

        // GET: api/Populations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Population>>> GetPopulation()
        {
            return await _context.Populations.ToListAsync();
        }

        // GET: api/Populations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Population>> GetPopulation(int? id)
        {
            var population = await _context.Populations.FindAsync(id);

            if (population == null)
            {
                return NotFound();
            }

            return population;
        }

        // PUT: api/Populations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPopulation( Population[] populations)
        {
            if (populations == null || populations.Length ==0 )
            {
                return BadRequest();
            }
            foreach (var pop in populations)
            {
                _context.Entry(pop).State = EntityState.Modified;
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

        // POST: api/Populations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Population>> PostPopulation(Population population)
        {
            population.Id = null;
            _context.Populations.Add(population);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPopulation", new { id = population.Id }, population);
        }

        // DELETE: api/Populations/5
        [HttpDelete]
        public async Task<IActionResult> DeletePopulations([FromBody]int[] ids)
        {
            foreach (var id in ids)
            {
                var population = await _context.Populations.FindAsync(id);
                if (population == null)
                {
                    return NotFound();
                }

                _context.Populations.Remove(population);
                await _context.SaveChangesAsync();
            }


           

            return Ok();
        }

        private bool PopulationExists(int? id)
        {
            return _context.Populations.Any(e => e.Id == id);
        }
    }
}
