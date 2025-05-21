using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.Auth;
using Microsoft.AspNetCore.Authorization;


namespace Wg_backend_api.Controllers.GlobalControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GlobalDbContext _context;

        public AuthController(GlobalDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CustomLoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(p => p.Name == request.Name);

            //if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) TODO 
            if (user == null || request.Password!= user.Password)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "Wrong username or password"
                });
            }
            if (user.IsArchived) {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User has been archived"
                });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            return Ok(new
            {
                message = "Login successful"
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            HttpContext.Session.Remove("Schema");


            return Ok(new
            {
                message = "Logout successful"
            });
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public IActionResult Status()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Ok(new
                {
                    isAuthenticated = true,
                    username = User.Identity.Name
                });
            }

            return Ok(new
            {
                isAuthenticated = false
            });
        }
    }
}
