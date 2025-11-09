namespace Wg_backend_api.Controllers.GlobalControllers
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Wg_backend_api.Auth;
    using Wg_backend_api.Data;
    using Wg_backend_api.Models;
    using Wg_backend_api.Services;

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GlobalDbContext _context;
        private readonly IConfiguration _config;
        private readonly ISessionDataService _sessionDataService;

        public AuthController(GlobalDbContext context, IConfiguration config, ISessionDataService sessionDataService)
        {
            this._context = context;
            this._config = config;
            this._sessionDataService = sessionDataService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CustomLoginRequest request)
        {
            var user = this._context.Users.FirstOrDefault(p => p.Name == request.Name || p.Email == request.Name);

            if (user == null)
            {
                return this.Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "Wrong username or password",
                });
            }

            if (!this.IsUserEligibleForLogin(user, request.Password, out var errorMessage))
            {
                return this.Unauthorized(new
                {
                    error = "Unauthorized",
                    message = errorMessage,
                });
            }

            var playerId = user.Id;
            var accessToken = this.GenerateJwtToken((int)playerId);
            var refreshToken = this.GenerateRefreshToken();

            this._context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken.ToString(),
                UserId = user.Id.Value,
                ExpiresAt = DateTime.UtcNow.AddDays(this._config.GetValue<int>("Jwt:RefreshTokenLifetimeDays")),
            });

            await this._context.SaveChangesAsync();

            return this.Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        {
            var result = await this._context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == req.RefreshToken);

            if (result == null || result.ExpiresAt <= DateTime.UtcNow || result.RevokedAt != null)
            {
                return this.Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "Invalid or expired refresh token",
                });
            }

            var user = await this._context.Users.FirstOrDefaultAsync(u => u.Id == result.UserId);
            if (user == null)
            {
                return this.Unauthorized(new
                {
                    error = "Unauthorized",
                    message = "User not found",
                });
            }

            var accessToken = this.GenerateJwtToken(user.Id.Value);
            var newRefreshToken = this.GenerateRefreshToken();

            result.RevokedAt = DateTime.UtcNow;
            this._context.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id.Value,
                ExpiresAt = DateTime.UtcNow.AddDays(this._config.GetValue<int>("Jwt:RefreshTokenLifetimeDays")),
            });

            await this._context.SaveChangesAsync();
            return this.Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest req) {
            var result = await this._context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == req.RefreshToken);
            if (result != null && result.RevokedAt == null)
            {
                result.RevokedAt = DateTime.UtcNow;
                await this._context.SaveChangesAsync();
            }

            this.HttpContext.Session.Clear();

            return this.Ok();
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            if (this.User.Identity != null && this.User.Identity.IsAuthenticated)
            {
                return this.Ok(new
                {
                    isAuthenticated = true,
                    username = this.User.Identity.Name,
                });
            }

            return this.Ok(new
            {
                isAuthenticated = false,
            });
        }

        // TODO add enpoint me that returns user access
        // TODO ensure we want this endpoint
        // [AllowAnonymous]
        // [HttpGet("me")]
        // public async Task<IActionResult> StatusMe()
        // {
        //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //     if (userId == null)
        //         return Unauthorized();

        //     var game = _sessionDataService.GetSchema();

        //     if (game == null)
        //     {
        //         return Ok(new
        //         {
        //             isAuthenticated = true,
        //             username = User.Identity.Name
        //         });
        //     }
        //     else
        //     {
        //         var nation = _sessionDataService.GetNation();
        //         var playerRole = _context.GameAccesses.FirstOrDefault(a => a.GameId.ToString() == game && a.UserId.ToString() == userId)?.Role;
        //         if (nation != null)
        //         {
        //             return Ok(new
        //             {
        //                 isAuthenticated = true,
        //                 username = User.Identity.Name,
        //                 role = playerRole
        //             });
        //         }
        //         else
        //         {
        //             return Ok(new
        //             {
        //                 isAuthenticated = true,
        //                 username = User.Identity.Name,
        //                 role = playerRole
        //             });
        //         }
        //     }
        // }

        // [AllowAnonymous]
        // [HttpGet("google-login")]
        // public IActionResult GoogleLogin(string returnUrl = "http://localhost:4200/loggedin")
        // {
        //     var props = new AuthenticationProperties
        //     {
        //         RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
        //     };

        //     return Challenge(props, "Google");
        // }

        // [AllowAnonymous]
        // [HttpGet("google-callback")]
        // public async Task<IActionResult> GoogleCallback(string returnUrl)
        // {
        //     var result = await HttpContext.AuthenticateAsync("Google");

        //     if (!result.Succeeded) return Unauthorized();

        //     var externalUser = result.Principal;
        //     var email = externalUser.FindFirst(ClaimTypes.Email)?.Value;
        //     if (email == null)
        //     {
        //         return BadRequest(new { error = "Email not found in external user data" });
        //     }

        //     var user = _context.Users.FirstOrDefault(u => u.Email == email);
        //     var register = false;
        //     if (user == null)
        //     {
        //         register = true;
        //         string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        //         user = new User
        //         {
        //             Name = $"user{base64Guid}",
        //             Email = email,
        //             Password = BCrypt.Net.BCrypt.HashPassword(""),
        //             IsSSO = true,
        //             IsArchived = false,
        //         };
        //         _context.Users.Add(user);
        //         await _context.SaveChangesAsync();

        //         string[] parts_url = returnUrl.Split('/');
        //         if (parts_url.Length > 3)
        //         {
        //             returnUrl = string.Join("/", parts_url.Take(3)) + "/set-username";
        //         }
        //         // TODO else
        //     }
        //     else if (user.IsArchived)
        //     {
        //         return Unauthorized(new { error = "User is archived" });
        //     }

        //     var claims = new List<Claim>
        //     {
        //         new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //         new Claim(ClaimTypes.Name, user.Name),
        //     };
        //     Console.WriteLine(user.Id.ToString());

        //     var identity = new ClaimsIdentity(claims, "MyCookieAuth");
        //     await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(identity));

        //     return Redirect(returnUrl);
        // }

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

        private string GenerateJwtToken(int playerId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, playerId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, playerId.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: this._config["Jwt:Issuer"],
                audience: this._config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(this._config.GetValue<int>("Jwt:TokenLifetime")), // TODO set appropriate expiration
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken() {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
