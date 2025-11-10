using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Wg_backend_api.Services;
using Wg_backend_api.Controllers.GlobalControllers;

public class UserIdActionFilter : IActionFilter
{
    private readonly ISessionDataService _sessionDataService;

    public UserIdActionFilter(ISessionDataService sessionDataService)
    {
        _sessionDataService = sessionDataService;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is PlayersController || context.Controller is GamesController || context.Controller is UserController)
        {
            var userIdStr = _sessionDataService.GetUserIdItems();
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
