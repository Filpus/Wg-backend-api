using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
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
            "/api/games/players", // TODO ensure if this is needed
        ];

        public GameAccessMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context, GlobalDbContext db, ISessionDataService sessionDataService)
        {
            var path = context.Request.Path;

            if (!path.StartsWithSegments("/api/auth"))
            {
                // TODO ensure we dont need to check id in every middleware call
                var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = int.TryParse(userIdStr, out var uid) ? uid : -1;

                sessionDataService.SetUserIdItems(userIdStr);

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
                    var gameAccess = await db.GameAccesses.FirstOrDefaultAsync(ga => ga.UserId == userId && ga.GameId == gameId);

                    if (gameAccess == null)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("No Access to Game");
                        return;
                    }

                    if (gameAccess.Role == UserRole.Player)
                    {
                        if (gameAccess.IsArchived)
                        {
                            context.Response.StatusCode = 403;
                            await context.Response.WriteAsync("User is archived in Game");
                            return;
                        }

                        if (gameAccess.NationName == null)
                        {
                            context.Response.StatusCode = 403;
                            await context.Response.WriteAsync("User has no assigned nation in game");
                            return;
                        }
                    }

                    context.Items["RoleInGame"] = gameRole;
                }
            }

            await this._next(context);
        }
    }
}
