using Microsoft.AspNetCore.Mvc;

namespace Wg_backend_api.Helpers
{

    public static class ControllerValidationExtensions
    {
        public static ActionResult? ValidateIdsProvided(this ControllerBase controller, System.Collections.Generic.List<int?>? ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return controller.BadRequest("Brak ID do usunięcia.");
            }

            return null;
        }
        public static ActionResult? NotFoundIfNoneFound<T>(this ControllerBase controller, System.Collections.Generic.List<T> found, string entityNamePlural)
        {
            if (found.Count == 0)
            {
                return controller.NotFound($"Nie znaleziono {entityNamePlural} do usunięcia.");
            }

            return null;
        }
    }
}
