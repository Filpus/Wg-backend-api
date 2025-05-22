using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentsController : ControllerBase
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public AssignmentsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        // GET: api/Assignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignment()
        {
            return await _context.Assignments.ToListAsync();
        }

        // GET: api/Assignments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetAssignment(int? id)
        {
            string schema = HttpContext.Session.GetString("Shema");
            if (string.IsNullOrEmpty(schema))
            {
                return BadRequest("Schema is null or empty");
            }
            var _context = _gameDbContextFactory.Create(schema);
            var assignment = await _context.Assignments.FindAsync(id);


            if (assignment == null)
            {
                return NotFound();
            }

            return assignment;
        }

        // PUT: api/Assignments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutAssignment( [FromBody] Assignment[] assignments)
        {
            foreach (var assignment in assignments)
            {
                _context.Entry(assignment).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentExists(assignment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return NoContent();
        }

        // POST: api/Assignments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Assignment>> PostAssignment(Assignment[] assignments)
        {
            foreach (var assignment in assignments)
            {
                assignment.Id = null;

                if (assignment.UserId >= 0 && assignment.NationId >= 0)
                {

                    _context.Assignments.Add(assignment);
                    await _context.SaveChangesAsync();
                }
                else {
                    return BadRequest();
                }
            }
            return Ok();
        }

        // DELETE: api/Assignments/5
        [HttpDelete]
        public async Task<IActionResult> DeleteAssignment([FromBody]int[] ids)
        {

            foreach (var id in ids)
            {
                var assignment = await _context.Assignments.FindAsync(id);
                if (assignment == null)
                {
                    return NotFound();
                }

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        private bool AssignmentExists(int? id)
        {
            return _context.Assignments.Any(e => e.Id == id);
        }
    }
}
