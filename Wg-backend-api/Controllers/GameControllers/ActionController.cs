using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/Actions")]
    [AuthorizeGameRole("GameMaster", "Player")]
    public class ActionController : GameControllerBase
    {
        public ActionController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
            : base(gameDbFactory, sessionDataService)
        {
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<ActionDTO>>> GetActions(int? id)
        {
            if (id.HasValue)
            {
                var action = await this.Context.Actions.FindAsync(id);
                if (action == null)
                {
                    return NotFound();
                }

                return Ok(new List<ActionDTO> { MapToDTO(action) });
            }
            else
            {
                var actions = await this.Context.Actions.ToListAsync();
                return Ok(actions.Select(MapToDTO));
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutActions([FromBody] List<ActionDTO> actionDTOs)
        {
            if (actionDTOs == null || actionDTOs.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var actionDTO in actionDTOs)
            {
                var action = await this.Context.Actions.FindAsync(actionDTO.Id);
                if (action == null)
                {
                    return NotFound($"Nie znaleziono akcji o ID {actionDTO.Id}.");
                }

                UpdateModelFromDTO(action, actionDTO);
                this.Context.Entry(action).State = EntityState.Modified;
            }

            try
            {
                await this.Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<List<ActionDTO>>> PostActions([FromBody] List<ActionDTO> actionDTOs)
        {
            if (actionDTOs == null || actionDTOs.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            var actions = actionDTOs.Select(dto => MapFromDTO(new ActionDTO(dto) { NationId = this.NationId ?? dto.NationId })).ToList();
            this.Context.Actions.AddRange(actions);
            await this.Context.SaveChangesAsync();

            return CreatedAtAction("GetActions", new { id = actions[0].Id }, actions.Select(MapToDTO));
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteActions([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var actions = await this.Context.Actions.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (actions.Count == 0)
            {
                return NotFound("Nie znaleziono akcji do usunięcia.");
            }

            this.Context.Actions.RemoveRange(actions);
            await this.Context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("settledAndUnsetled")]
        public async Task<ActionResult> GetSettledAndUnsettledActions()
        {
            var settledActions = await this.Context.Actions
                .Where(a => a.IsSettled)
                .ToListAsync();

            var unsettledActions = await this.Context.Actions
                .Where(a => !a.IsSettled)
                .ToListAsync();

            return Ok(new
            {
                SettledActions = settledActions.Select(MapToDTO),
                UnsettledActions = unsettledActions.Select(MapToDTO)
            });
        }

        [HttpGet("settled/{nationId?}")]
        public async Task<ActionResult> GetSettledActionsForNation(int? nationId)
        {
            if (!nationId.HasValue)
            {
                nationId = this.NationId;
            }

            var settledActions = await this.Context.Actions
                .Where(a => a.IsSettled && a.NationId == nationId)
                .ToListAsync();

            return Ok(settledActions.Select(MapToDTO));
        }

        [HttpGet("unsettled/{nationId?}")]
        public async Task<ActionResult> GetUnsettledActionsForNation(int? nationId)
        {
            if (!nationId.HasValue)
            {
                nationId = this.NationId;
            }

            var unsettledActions = await this.Context.Actions
                .Where(a => !a.IsSettled && a.NationId == nationId)
                .ToListAsync();

            return Ok(unsettledActions.Select(MapToDTO));
        }

        private ActionDTO MapToDTO(Models.Action action)
        {
            return new ActionDTO
            {
                Id = action.Id,
                NationId = action.NationId,
                Name = action.Name,
                Description = action.Description,
                Result = action.Result,
                IsSettled = action.IsSettled
            };
        }

        private Models.Action MapFromDTO(ActionDTO dto)
        {
            return new Models.Action
            {
                NationId = dto.NationId,
                Name = dto.Name,
                Description = dto.Description,
                Result = dto.Result,
                IsSettled = dto.IsSettled
            };
        }

        private void UpdateModelFromDTO(Models.Action action, ActionDTO dto)
        {
            action.NationId = dto.NationId;
            action.Name = dto.Name;
            action.Description = dto.Description;
            action.Result = dto.Result;
            action.IsSettled = dto.IsSettled;
        }
    }
}
