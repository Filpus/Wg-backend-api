using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Wg_backend_api.Services;
using Wg_backend_api.Controllers.GameControllers;
using Wg_backend_api.Controllers.GlobalControllers;

public class UserIdActionFilter : IActionFilter
{
    private readonly ISessionDataService _sessionDataService;

    public UserIdActionFilter(ISessionDataService sessionDataService)
    {
        _sessionDataService = sessionDataService;
        Console.WriteLine("UserIdActionFilter executed before action.");
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if(context.Controller is PlayersController || context.Controller is GamesController)
        {
            var userIdStr = _sessionDataService.GetUserId();
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out _))
            {
                context.Result = new UnauthorizedResult();
            }
            Console.WriteLine("UserIdActionFilter executed before action.");
            // TODO fuszera drut, create class for controller
            if (context.Controller is PlayersController playersController)
            {
                int userId = int.Parse(userIdStr);
                playersController.SetUserId(userId);
                Console.WriteLine("UserIdActionFilter executed for PlayersController.");
            }
            else if (context.Controller is GamesController gamesController)
            {
                int userId = int.Parse(userIdStr);
                gamesController.SetUserId(userId);
                Console.WriteLine("UserIdActionFilter executed for GamesController.");
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
