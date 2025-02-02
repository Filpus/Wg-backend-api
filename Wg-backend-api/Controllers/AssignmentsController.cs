using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssignmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Assignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignment()
        {
            return await _context.Assignment.ToListAsync();
        }

        // GET: api/Assignments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetAssignment(int? id)
        {
            var assignment = await _context.Assignment.FindAsync(id);

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

                    _context.Assignment.Add(assignment);
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
                var assignment = await _context.Assignment.FindAsync(id);
                if (assignment == null)
                {
                    return NotFound();
                }

                _context.Assignment.Remove(assignment);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        private bool AssignmentExists(int? id)
        {
            return _context.Assignment.Any(e => e.Id == id);
        }
    }
}
