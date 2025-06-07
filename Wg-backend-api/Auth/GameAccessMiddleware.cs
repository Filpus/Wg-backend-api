using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wg_backend_api.Data;

namespace Wg_backend_api.Auth
{
    public class GameAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] ExcludedPaths =
        {
            "/api/auth",
            "/api/games",
            "/api/user",
            "/api/players"
        }; 

        public GameAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, GlobalDbContext db, IGameDbContextFactory gameFactory)
        {
            var path = context.Request.Path;

            //if (!ExcludedPaths.Any(p => path.StartsWithSegments(p)))
            //{
            //    var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            //    var gameIdStr = context.Session.GetString("Schema");
            //    var nationIdStr = context.Session.GetString("Nation"); // TODO add check for nationIdStr 

            //    if (string.IsNullOrEmpty(gameIdStr) || !gameIdStr.StartsWith("game_"))
            //    {
            //        context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //        await context.Response.WriteAsync("Forbidden - invalid game access.");
            //        return;
            //    }
            //    var idPart = gameIdStr.Replace("game_", "");

            //    if (!int.TryParse(userIdStr, out int userId) || !int.TryParse(gameIdStr, out int gameId))
            //    {
            //        context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //        await context.Response.WriteAsync("Forbidden - invalid game access.");
            //        return;
            //    }

            //    var hasAccess = await db.GameAccesses
            //        .AnyAsync(a => a.UserId == userId && a.GameId == gameId);

            //    if (!hasAccess)
            //    {
            //        context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //        await context.Response.WriteAsync("Forbidden - no access to this game.");
            //        return;
            //    }
            //}

            await _next(context);
        }
    }

}
