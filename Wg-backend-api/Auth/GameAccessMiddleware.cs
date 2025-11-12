using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.Services;

namespace Wg_backend_api.Auth
{
    public class GameAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] ExcludedPaths =
        [
            "/api/auth",
            "/api/games",
            "/api/user",
            "/api/players", // TODO ensure if this is needed
        ];

        public GameAccessMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context, GlobalDbContext db, IGameDbContextFactory gameFactory, ISessionDataService sessionDataService)
        {
            var path = context.Request.Path;

            if (!path.StartsWithSegments("/api/auth"))
            {
                // TODO ensure we dont need to check id in every middleware call
                var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdStr))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                sessionDataService.SetUserIdItems(userIdStr);
            }

            if (!ExcludedPaths.Any(p => path.StartsWithSegments(p)))
            {
                var gameIdHeader = sessionDataService.GetSchema();
                if (string.IsNullOrEmpty(gameIdHeader))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Missing Game Schema");
                    return;
                }

                var gameRole = sessionDataService.GetRole();
                if (string.IsNullOrEmpty(gameRole))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Missing Role in Game");
                    return;
                }

                var gameId = int.Parse(gameIdHeader.Replace("game_", string.Empty));
                context.Items["RoleInGame"] = gameRole;
            }

            await this._next(context);
        }
    }
}
