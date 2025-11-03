using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.Services;

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

        public async Task InvokeAsync(HttpContext context, GlobalDbContext db, IGameDbContextFactory gameFactory, ISessionDataService sessionDataService)
        {
            var path = context.Request.Path;

            // var claims = context.User.Claims;

            // Console.WriteLine("Claims in token:");
            // foreach (var claim in claims)
            // {
            //     Console.WriteLine($"{claim.Type}: {claim.Value}");
            // }

            // var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            // Console.WriteLine($"Authorization header: {authHeader}");

            // if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            // {
            //     var token = authHeader.Substring("Bearer ".Length);
            //     var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            //     var jwtToken = handler.ReadJwtToken(token);

            //     Console.WriteLine("Claims in raw token:");
            //     foreach (var claim in jwtToken.Claims)
            //     {
            //         Console.WriteLine($"{claim.Type}: {claim.Value}");
            //     }
            // }
            // else
            // {
            //     Console.WriteLine("No Authorization header found.");
            // }
            if (!ExcludedPaths.Any(p => path.StartsWithSegments(p)))
            {
                // var gameIdHeader = context.Request.Headers["X-Game-Id"].FirstOrDefault();
                var gameIdHeader = sessionDataService.GetSchema();
                Console.WriteLine($"GameId from session: {gameIdHeader}");
                var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdStr))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                if (string.IsNullOrEmpty(gameIdHeader))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Missing X-Game-Id header");
                    return;
                }

                var userId = int.Parse(userIdStr);
                var gameId = int.Parse(gameIdHeader.Replace("game_", string.Empty));
                var hasAccess = await db.GameAccesses.FirstOrDefaultAsync(a => a.UserId == userId && a.GameId == gameId);

                if (hasAccess == null)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("No access to this game");
                    return;
                }

                context.Items["GameSchema"] = $"game_{gameId}";
                context.Items["RoleInGame"] = hasAccess.Role;
            }
            else if (path.StartsWithSegments("/api/games"))
            {
                var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine($"UserId from token: {userIdStr}");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                context.Items["UserId"] = int.Parse(userIdStr);
            }

            await _next(context);
        }
    }

}
