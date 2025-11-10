
namespace Wg_backend_api.Auth
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class AuthorizeGameRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeGameRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Items["RoleInGame"]?.ToString();
            Console.WriteLine($"AuthorizeGameRoleAttribute: User role in game is '{role}'.");

            if (string.IsNullOrEmpty(role) || !_roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}