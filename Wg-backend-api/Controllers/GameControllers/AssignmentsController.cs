using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class AssignmentsController : ControllerBase
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public AssignmentsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
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
        // GET: api/Assignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignment()
        {
            return await this._context.Assignments.ToListAsync();
        }

        // GET: api/Assignments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetAssignment(int? id)
        {
            var assignment = await this._context.Assignments.FindAsync(id);

            if (assignment == null)
            {
                return NotFound();
            }

            return assignment;
        }

        // PUT: api/Assignments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutAssignment([FromBody] Assignment[] assignments)
        {
            foreach (var assignment in assignments)
            {
                this._context.Entry(assignment).State = EntityState.Modified;

                try
                {
                    await this._context.SaveChangesAsync();
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

                    this._context.Assignments.Add(assignment);
                    await this._context.SaveChangesAsync();
                }
                else
                {
                    return BadRequest();
                }
            }

            return Ok();
        }

        // DELETE: api/Assignments/5
        [HttpDelete]
        public async Task<IActionResult> DeleteAssignment([FromBody] int[] ids)
        {

            foreach (var id in ids)
            {
                var assignment = await this._context.Assignments.FindAsync(id);
                if (assignment == null)
                {
                    return NotFound();
                }

                this._context.Assignments.Remove(assignment);
                await this._context.SaveChangesAsync();
            }

            return NoContent();
        }

        private bool AssignmentExists(int? id)
        {
            return this._context.Assignments.Any(e => e.Id == id);
        }
    }
}
