
namespace Wg_backend_api.Auth
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class AuthorizeGameRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _requiredRole;

        public AuthorizeGameRoleAttribute(string requiredRole)
        {
            this._requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Items["RoleInGame"]?.ToString();

            if (string.IsNullOrEmpty(role) || !role.Equals(this._requiredRole, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
            }
        }
    }

}