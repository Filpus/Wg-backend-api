using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
using static Wg_backend_api.DTO.NationsWithAssignmentsDTO;

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

        // GET: api/Assignments/detailed
        [HttpGet("nations")]
        public async Task<ActionResult<List<NationsWithAssignmentsDTO>>> GetDetailedAssignments()
        {
            var nations = await this._context.Nations
                .Select(n => new NationsWithAssignmentsDTO
                {
                    Id = n.Id,
                    Name = n.Name,
                    Color = n.Color,
                    Flag = n.Flag,
                    Assignment = n.Assignment != null ? new AssignmentInfoDTO
                    {
                        Id = n.Assignment.Id,
                        UserId = n.Assignment.UserId,
                        UserName = n.Assignment.User != null ? n.Assignment.User.Name : null,
                    }
                    : null,
                })
                .ToListAsync();

            return Ok(nations);
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
        public async Task<IActionResult> PostAssignment([FromBody] AssignmentDTO[] assignments)
        {
            foreach (var assignment in assignments)
            {
                var user = await this._context.Players.FindAsync(assignment.UserId);
                if (user == null || user.Role != UserRole.Player)
                {
                    return BadRequest("Invalid user for assignment.");
                }

                // TODO Temoprary settings one assignment per nation
                var existingAssignment = await this._context.Assignments
                    .Where(a => a.NationId == assignment.NationId && a.UserId == assignment.UserId)
                    .FirstOrDefaultAsync();
                if (existingAssignment != null)
                {
                    continue;
                }

                var existingUserAssignment = await this._context.Assignments
                    .Where(a => a.UserId == assignment.UserId)
                    .FirstOrDefaultAsync();
                if (existingUserAssignment != null)
                {
                    continue;
                }

                var nation = await this._context.Assignments.Where(a => a.NationId == assignment.NationId).FirstOrDefaultAsync();
                if (nation != null)
                {
                    this._context.Assignments.Remove(nation);
                    await this._context.SaveChangesAsync();
                }

                // End of temporary settings

                if (assignment.UserId >= 0 && assignment.NationId >= 0)
                {
                    var newAssignment = new Assignment
                    {
                        UserId = assignment.UserId,
                        NationId = assignment.NationId,
                        DateAcquired = DateTime.UtcNow,
                        IsActive = true,
                    };
                    this._context.Assignments.Add(newAssignment);
                    await this._context.SaveChangesAsync();
                }
                else
                {
                    return BadRequest();
                }
            }

            return Ok();
        }

        // DELETE: api/Assignments
        [HttpDelete]
        public async Task<IActionResult> DeleteAssignmentById([FromBody] int[] ids)
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

        [HttpDelete("by-assignment")]
        public async Task<IActionResult> DeleteAssignment([FromBody] AssignmentDTO[] assignments)
        {
            foreach (var assign in assignments)
            {
                var assignment = await this._context.Assignments
                    .Where(a => a.NationId == assign.NationId && a.UserId == assign.UserId)
                    .FirstOrDefaultAsync();

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
