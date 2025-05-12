using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.Auth;
using Wg_backend_api.Models;


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

        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string returnUrl = "http://localhost:4200/loggedin")
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
            };

            return Challenge(props, "Google");
        }
        
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string returnUrl)
        {
            var result = await HttpContext.AuthenticateAsync("Google");

            if (!result.Succeeded) return Unauthorized();

            var externalUser = result.Principal;
            var email = externalUser.FindFirst(ClaimTypes.Email)?.Value;
            var name = externalUser.Identity.Name; // TODO change name for prefix of email

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Name = name,
                    Email = email,
                    Password = ""
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            if (user.IsArchived)
            {
                return Unauthorized(new { error = "User is archived" });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("MyCookieAuth", principal);

            return Redirect(returnUrl);
        }


    }
}
