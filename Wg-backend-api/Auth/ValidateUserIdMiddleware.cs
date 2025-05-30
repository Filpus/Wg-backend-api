using System.Security.Claims;

namespace Wg_backend_api.Auth
{
    public class ValidateUserIdMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidateUserIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // If anonymous addnotation doesn't work

            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null)
            {
                await _next(context);
                return;
            }

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(userId, out _))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid UserId claim.");
                    return;
                }
            }

            await _next(context);
        }
    }

}
