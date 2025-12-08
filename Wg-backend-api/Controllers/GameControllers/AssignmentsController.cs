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
        private readonly GlobalDbContext _globalDbContext;

        public AssignmentsController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService, GlobalDbContext globalDbContext)
        {
            this._gameDbContextFactory = gameDbFactory;
            this._sessionDataService = sessionDataService;
            this._globalDbContext = globalDbContext;

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
            if (!TryGetGameId(out var gameId))
            {
                return BadRequest(new { error = "Bad Request", message = "No game selected in session" });
            }

            var gameAccess = await GetGameAccessAsync(gameId);

            foreach (var assignment in assignments)
            {
                var user = await this._context.Players.FindAsync(assignment.UserId);
                if (user == null || user.Role != UserRole.Player)
                {
                    return BadRequest("Invalid user for assignment.");
                }

                var nation = await this._context.Nations.FindAsync(assignment.NationId);
                if (nation == null)
                {
                    return BadRequest("Invalid nation for assignment.");
                }

                this._context.Entry(assignment).State = EntityState.Modified;
                gameAccess
                    .Where(ga => ga.UserId == user.UserId)
                    .ToList()
                    .ForEach(ga => ga.NationName = nation.Name);
                this._globalDbContext.GameAccesses.UpdateRange(gameAccess);

                try
                {
                    await this._globalDbContext.SaveChangesAsync();
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
            if (!TryGetGameId(out var gameId))
            {
                return BadRequest(new { error = "Bad Request", message = "No game selected in session" });
            }

            var gameAccess = await GetGameAccessAsync(gameId);

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

                var nationAssigmnet = await this._context.Assignments.Where(a => a.NationId == assignment.NationId).FirstOrDefaultAsync();
                if (nationAssigmnet != null)
                {
                    this._context.Assignments.Remove(nationAssigmnet);
                    await this._context.SaveChangesAsync();
                }

                var nation = await this._context.Nations.FindAsync(assignment.NationId);
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
                    this._context.Assignments.Add(newAssignment);
                    await this._context.SaveChangesAsync();
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
                var assignment = await this._context.Assignments.FindAsync(id);
                if (assignment == null)
                {
                    return NotFound();
                }

                var user = await this._context.Players.FindAsync(assignment.UserId);

                gameAccess
                    .Where(ga => ga.UserId == user.UserId)
                    .ToList()
                    .ForEach(ga => ga.NationName = null);
                this._globalDbContext.GameAccesses.UpdateRange(gameAccess);
                await this._globalDbContext.SaveChangesAsync();

                this._context.Assignments.Remove(assignment);
                await this._context.SaveChangesAsync();
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
                var assignment = await this._context.Assignments
                    .Where(a => a.NationId == assign.NationId && a.UserId == assign.UserId)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    return NotFound();
                }

                var user = await this._context.Players.FindAsync(assignment.UserId);

                gameAccess
                    .Where(ga => ga.UserId == user.UserId)
                    .ToList()
                    .ForEach(ga => ga.NationName = null);
                this._globalDbContext.GameAccesses.UpdateRange(gameAccess);
                await this._globalDbContext.SaveChangesAsync();

                this._context.Assignments.Remove(assignment);
                await this._context.SaveChangesAsync();
            }

            return NoContent();
        }

        private bool AssignmentExists(int? id)
        {
            return this._context.Assignments.Any(e => e.Id == id);
        }

        private bool TryGetGameId(out int gameId)
        {
            gameId = -1;
            var selectedGame = this._sessionDataService.GetSchema();
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
