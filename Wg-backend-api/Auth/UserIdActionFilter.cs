using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Wg_backend_api.Controllers.GlobalControllers;
using Wg_backend_api.Services;

public class UserIdActionFilter : IActionFilter
{
    private readonly ISessionDataService _sessionDataService;

    public UserIdActionFilter(ISessionDataService sessionDataService)
    {
        this._sessionDataService = sessionDataService;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is PlayersController or GamesController or UserController)
        {
            var userIdStr = this._sessionDataService.GetUserIdItems();
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out _))
            {
                context.Result = new UnauthorizedResult();
            }

            // TODO fuszera drut, create class for controller
            if (context.Controller is PlayersController playersController)
            {
                int userId = int.Parse(userIdStr);
                playersController.SetUserId(userId);
            }
            else if (context.Controller is GamesController gamesController)
            {
                int userId = int.Parse(userIdStr);
                gamesController.SetUserId(userId);
            }
            else if (context.Controller is UserController userController)
            {
                int userId = int.Parse(userIdStr);
                userController.SetUserId(userId);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
