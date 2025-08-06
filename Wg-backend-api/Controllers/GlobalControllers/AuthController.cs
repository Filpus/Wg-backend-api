using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wg_backend_api.Data;
using Wg_backend_api.Auth;
using Microsoft.AspNetCore.Authorization;
using Wg_backend_api.Models;
using System.IO;
using System.Threading.Tasks;
using BCrypt.Net;

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
            var user = _context.Users.FirstOrDefault(p => p.Name == request.Name || p.Email == request.Name);

            if (user == null)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "Wrong username or password"
                });
            }

            if (!IsUserEligibleForLogin(user, request.Password, out var errorMessage))
            {
                return Unauthorized(new
                {
                    error = "Unauthorized",
                    message = errorMessage
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
        public async Task<IActionResult> Status()
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
        [AllowAnonymous]
        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string returnUrl = "http://localhost:4200/loggedin")
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
            };

            return Challenge(props, "Google");
        }
        [AllowAnonymous]
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string returnUrl)
        {
            var result = await HttpContext.AuthenticateAsync("Google");

            if (!result.Succeeded) return Unauthorized();

            var externalUser = result.Principal;
            var email = externalUser.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) { 
                return BadRequest(new { error = "Email not found in external user data" });
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            var register = false;
            if (user == null)
            {
                register = true;
                string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                user = new User
                {
                    Name = $"user{base64Guid}",
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(""),
                    IsSSO = true,
                    IsArchived = false,
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                string[] parts_url = returnUrl.Split('/');
                if (parts_url.Length > 3)
                {
                    returnUrl = string.Join("/", parts_url.Take(3)) + "/set-username";
                }
                // TODO else
            }
            else if (user.IsArchived)
            {
                return Unauthorized(new { error = "User is archived" });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
            };
            Console.WriteLine(user.Id.ToString());

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(identity));

            return Redirect(returnUrl);
        }

        private bool IsUserEligibleForLogin(User user, string password, out string errorMessage)
        {
            if (user.IsSSO)
            {
                errorMessage = "User is SSO, please use SSO login";
                return false;
            }

            if (user.IsArchived)
            {
                errorMessage = "User has been archived";
                return false;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                errorMessage = "Wrong username or password";
                return false;
            }

            errorMessage = null;
            return true;
        }

    }
}
