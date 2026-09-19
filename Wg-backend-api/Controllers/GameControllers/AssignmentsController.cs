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
    [AuthorizeGameRole("GameMaster", "Player")]
    public class AssignmentsController : GameControllerBase
    {
        private readonly GlobalDbContext _globalDbContext;

        public AssignmentsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, GlobalDbContext globalDbContext)
            : base(gameDbFactory, sessionDataService)
        {
            this._globalDbContext = globalDbContext;
        }

        // GET: api/Assignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignment()
        {
            return await this.Context.Assignments.ToListAsync();
        }

        // GET: api/Assignments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetAssignment(int? id)
        {
            var assignment = await this.Context.Assignments.FindAsync(id);

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
            var nations = await this.Context.Nations
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
            if (!TryGetGameId(out var gameId))
            {
                return BadRequest(new { error = "Bad Request", message = "No game selected in session" });
            }

            var gameAccess = await GetGameAccessAsync(gameId);

            foreach (var assignment in assignments)
            {
                var user = await this.Context.Players.FindAsync(assignment.UserId);
                if (user == null || user.Role != UserRole.Player)
                {
                    return BadRequest("Invalid user for assignment.");
                }

                var nation = await this.Context.Nations.FindAsync(assignment.NationId);
                if (nation == null)
                {
                    return BadRequest("Invalid nation for assignment.");
                }

                this.Context.Entry(assignment).State = EntityState.Modified;
                gameAccess
                    .Where(ga => ga.UserId == user.UserId)
                    .ToList()
                    .ForEach(ga => ga.NationName = nation.Name);
                this._globalDbContext.GameAccesses.UpdateRange(gameAccess);

                try
                {
                    await this._globalDbContext.SaveChangesAsync();
                    await this.Context.SaveChangesAsync();
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
            if (!TryGetGameId(out var gameId))
            {
                return BadRequest(new { error = "Bad Request", message = "No game selected in session" });
            }

            var gameAccess = await GetGameAccessAsync(gameId);

            foreach (var assignment in assignments)
            {
                var user = await this.Context.Players.FindAsync(assignment.UserId);
                if (user == null || user.Role != UserRole.Player)
                {
                    return BadRequest("Invalid user for assignment.");
                }

                // TODO Temoprary settings one assignment per nation
                var existingAssignment = await this.Context.Assignments
                    .Where(a => a.NationId == assignment.NationId && a.UserId == assignment.UserId)
                    .FirstOrDefaultAsync();
                if (existingAssignment != null)
                {
                    continue;
                }

                var existingUserAssignment = await this.Context.Assignments
                    .Where(a => a.UserId == assignment.UserId)
                    .FirstOrDefaultAsync();
                if (existingUserAssignment != null)
                {
                    continue;
                }

                var nationAssigmnet = await this.Context.Assignments.Where(a => a.NationId == assignment.NationId).FirstOrDefaultAsync();
                if (nationAssigmnet != null)
                {
                    this.Context.Assignments.Remove(nationAssigmnet);
                    await this.Context.SaveChangesAsync();
                }

                var nation = await this.Context.Nations.FindAsync(assignment.NationId);
                if (nation == null)
                {
                    return BadRequest("Invalid nation for assignment.");
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
                    this.Context.Assignments.Add(newAssignment);
                    await this.Context.SaveChangesAsync();
                    gameAccess
                        .Where(ga => ga.UserId == user.UserId)
                        .ToList()
                        .ForEach(ga => ga.NationName = nation.Name);
                    this._globalDbContext.GameAccesses.UpdateRange(gameAccess);
                    await this._globalDbContext.SaveChangesAsync();
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
            if (!TryGetGameId(out var gameId))
            {
                return BadRequest(new { error = "Bad Request", message = "No game selected in session" });
            }

            var gameAccess = await GetGameAccessAsync(gameId);

            foreach (var id in ids)
            {
                var assignment = await this.Context.Assignments.FindAsync(id);
                if (assignment == null)
                {
                    return NotFound();
                }

                var user = await this.Context.Players.FindAsync(assignment.UserId);

                gameAccess
                    .Where(ga => ga.UserId == user.UserId)
                    .ToList()
                    .ForEach(ga => ga.NationName = null);
                this._globalDbContext.GameAccesses.UpdateRange(gameAccess);
                await this._globalDbContext.SaveChangesAsync();

                this.Context.Assignments.Remove(assignment);
                await this.Context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("by-assignment")]
        public async Task<IActionResult> DeleteAssignment([FromBody] AssignmentDTO[] assignments)
        {
            if (!TryGetGameId(out var gameId))
            {
                return BadRequest(new { error = "Bad Request", message = "No game selected in session" });
            }

            var gameAccess = await GetGameAccessAsync(gameId);

            foreach (var assign in assignments)
            {
                var assignment = await this.Context.Assignments
                    .Where(a => a.NationId == assign.NationId && a.UserId == assign.UserId)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return NotFound();
                }

                var user = await this.Context.Players.FindAsync(assignment.UserId);

                gameAccess
                    .Where(ga => ga.UserId == user.UserId)
                    .ToList()
                    .ForEach(ga => ga.NationName = null);
                this._globalDbContext.GameAccesses.UpdateRange(gameAccess);
                await this._globalDbContext.SaveChangesAsync();

                this.Context.Assignments.Remove(assignment);
                await this.Context.SaveChangesAsync();
            }

            return NoContent();
        }

        private bool AssignmentExists(int? id)
        {
            return this.Context.Assignments.Any(e => e.Id == id);
        }

        private bool TryGetGameId(out int gameId)
        {
            gameId = -1;
            var selectedGame = this.SessionDataService.GetSchema();
            if (string.IsNullOrEmpty(selectedGame) || !selectedGame.StartsWith("game_"))
            {
                return false;
            }

            gameId = int.Parse(selectedGame.Split('_')[1]);
            return true;
        }

        private Task<List<GameAccess>> GetGameAccessAsync(int gameId)
        {
            return this._globalDbContext.GameAccesses
                .Where(ga => ga.GameId == gameId)
                .ToListAsync();
        }
    }
}
