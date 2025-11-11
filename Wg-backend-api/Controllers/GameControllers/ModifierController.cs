using Microsoft.AspNetCore.Mvc;
using Wg_backend_api.Logic.Modifiers;

namespace Wg_backend_api.Controllers.GameControllers
{
    public class ModifierController : Controller
    {
        private ModifierManager _modifierManager;

        public void ModifiersController(ModifierManager modifierManager)
        {
            this._modifierManager = modifierManager;
        }

        [HttpPost("apply-event")]
        public async Task<ActionResult> ApplyEvent([FromBody] ApplyEventRequest request)
        {
            var results = await this._modifierManager.ApplyEventToNationAsync(request.EventId, request.NationId);
            return Ok(results);
        }
    }

    public class ApplyEventRequest
    {
        public int EventId { get; set; }
        public int NationId { get; set; }
    }
}

