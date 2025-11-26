using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GlobalControllers
{
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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = this._context.Users.FirstOrDefault(p => p.Email == request.Email || p.Name == request.Email);

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
            var accessToken = this.GenerateJwtToken((int)playerId, user.Name);
            var refreshToken = this.GenerateRefreshToken();

            this._context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken.ToString(),
                UserId = user.Id.Value,
                ExpiresAt = DateTime.UtcNow.AddDays(this._config.GetValue<int>("Jwt:RefreshTokenLifetimeDays")),
            });

            await this._context.SaveChangesAsync();

            this.SetAuthCookies(accessToken, refreshToken);

            return Ok(new { message = "Login successful", username = user.Name });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = this._context.Users.FirstOrDefault(p => p.Email == request.Email || p.Name == request.Name);

            if (user != null)
            {
                return this.Conflict(new
                {
                    error = "Conflict",
                    message = user.Name == request.Name ? "User with this username already exists" : "User with this email already exists",
                });
            }

            if (!ValidateUserData.isValidEmail(request.Email))
            {
                return this.BadRequest(new
                {
                    error = "BadRequest",
                    message = "Invalid email format",
                });
            }

            if (!ValidateUserData.isValidUsername(request.Name))
            {
                return this.BadRequest(new
                {
                    error = "BadRequest",
                    message = "Invalid username format. Username must be 3-40 characters long and can contain letters, digits, underscores, and hyphens.",
                });
            }

            user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsSSO = false,
                IsArchived = false,
            };

            this._context.Users.Add(user);
            await this._context.SaveChangesAsync();

            var playerId = user.Id;
            var accessToken = this.GenerateJwtToken((int)playerId, user.Name);
            var refreshToken = this.GenerateRefreshToken();

            this._context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken.ToString(),
                UserId = user.Id.Value,
                ExpiresAt = DateTime.UtcNow.AddDays(this._config.GetValue<int>("Jwt:RefreshTokenLifetimeDays")),
            });

            await this._context.SaveChangesAsync();

            this.SetAuthCookies(accessToken, refreshToken);

            return Ok(new { message = "Registered successful" });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        {
            var oldRefreshToken = this.Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(oldRefreshToken))
            {
                return Unauthorized(new { error = "No refresh token provided" });
            }

            var result = await this._context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == oldRefreshToken);
            if (result == null || result.ExpiresAt <= DateTime.UtcNow || result.RevokedAt != null)
            {
                return Unauthorized(new { error = "Invalid or expired refresh token" });
            }

            var user = await this._context.Users.FirstOrDefaultAsync(u => u.Id == result.UserId);
            if (user == null)
            {
                return Unauthorized(new { error = "User not found" });
            }

            var newAccessToken = GenerateJwtToken(user.Id.Value, user.Name);
            var newRefreshToken = GenerateRefreshToken();

            result.RevokedAt = DateTime.UtcNow;
            this._context.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id.Value,
                ExpiresAt = DateTime.UtcNow.AddDays(this._config.GetValue<int>("Jwt:RefreshTokenLifetimeDays"))
            });

            await this._context.SaveChangesAsync();

            SetAuthCookies(newAccessToken, newRefreshToken);

            return Ok(new { message = "Token refreshed" });
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
        {
            this.Response.Cookies.Delete("access_token");
            this.Response.Cookies.Delete("refresh_token");

            this.HttpContext.Session.Clear();
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            if (this.User.Identity != null && this.User.Identity.IsAuthenticated)
            {
                var nation = this._sessionDataService.GetNation();
                var game = this._sessionDataService.GetSchema();
                var playerRole = this._sessionDataService.GetRole();
                var playerIdStr = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var playerId = playerIdStr != null ? int.Parse(playerIdStr) : (int?)null;
                if (nation != null && game != null && playerRole != null && playerId != null)
                {
                    return this.Ok(new
                    {
                        isAuthenticated = true,
                        username = this._context.Users.Find(playerId)?.Name,
                        nation,
                        schema = game,
                        role = playerRole,
                    });
                }

                return this.Ok(new
                {
                    isAuthenticated = true,
                    username = playerId != null ? this._context.Users.Find(playerId)?.Name : this.User.FindFirst(ClaimTypes.Name)?.Value,
                });
            }

            return this.Ok(new
            {
                isAuthenticated = false,
            });
        }

        [AllowAnonymous]
        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string returnUrl = "https://localhost:4200/loggedin")
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = this.Url.Action(nameof(GoogleCallback), new { returnUrl })
            };

            return Challenge(props, "Google");
        }

        [AllowAnonymous]
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string returnUrl)
        {
            await this.HttpContext.SignOutAsync("External");
            var result = await this.HttpContext.AuthenticateAsync("Google");

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var externalUser = result.Principal;
            var email = externalUser.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return BadRequest(new { error = "Email not found in external user data" });
            }

            var user = this._context.Users.FirstOrDefault(u => u.Email == email);
            var register = false;
            if (user == null)
            {
                register = true;
                string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                user = new User
                {
                    Name = $"user{base64Guid}",
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(string.Empty),
                    IsSSO = true,
                    IsArchived = false,
                };

                this._context.Users.Add(user);
                await this._context.SaveChangesAsync();
                returnUrl = returnUrl.Replace("loggedin", "set-username");
            }
            else if (user.IsArchived)
            {
                return this.Unauthorized(new { error = "User is archived" });
            }

            var accessToken = this.GenerateJwtToken((int)user.Id, user.Name);
            var refreshToken = this.GenerateRefreshToken();

            this._context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken.ToString(),
                UserId = user.Id.Value,
                ExpiresAt = DateTime.UtcNow.AddDays(this._config.GetValue<int>("Jwt:RefreshTokenLifetimeDays")),
            });

            await this._context.SaveChangesAsync();

            this.SetAuthCookies(accessToken, refreshToken);

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

        private string GenerateJwtToken(int playerId, string userName)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, playerId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, playerId.ToString()),
                new Claim(ClaimTypes.Name, userName),
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

        private string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None, // SameSiteMode.Strict
                Expires = DateTime.UtcNow.AddMinutes(this._config.GetValue<int>("Jwt:TokenLifetime")),
            };

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None, // SameSiteMode.Strict
                Expires = DateTime.UtcNow.AddDays(this._config.GetValue<int>("Jwt:RefreshTokenLifetimeDays")),
            };

            this.Response.Cookies.Append("access_token", accessToken, accessCookieOptions);
            this.Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
        }
    }
}
