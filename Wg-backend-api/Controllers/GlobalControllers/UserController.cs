using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers.GlobalControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly GlobalDbContext _context;
        private int _userId;

        public UserController(GlobalDbContext context)
        {
            this._context = context;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetUserId(int userId)
        {
            this._userId = userId;
        }

        [Authorize]
        [HttpGet]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<ActionResult<UserDTO>> GetUser()
        {
            var user = await this._context.Users.FindAsync(this._userId);

            if (user == null)
            {
                return this.NotFound(new { message = "User not found." });
            }

            return this.Ok(new UserDTO
            {
                Name = user.Name,
                Email = user.Email,
                IsSSO = user.IsSSO,
                Image = user.Image,
            });
        }

        [Authorize]
        [HttpGet("{id}")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            if (this._userId != id)
            {
                return this.Unauthorized(new { message = "Unauthorized access: User ID mismatch." });
            }

            var user = await this._context.Users.FindAsync(id);

            if (user == null)
            {
                return this.NotFound(new { message = "User not found." });
            }

            return this.Ok(new UserDTO
            {
                Name = user.Name,
                Email = user.Email,
                IsSSO = user.IsSSO,
                Image = user.Image,
            });
        }

        // Edit user data
        [Authorize]
        [HttpPut("{id}")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> PutUser(int? id, User user)
        {
            if (this._userId != id)
            {
                return this.Unauthorized(new { message = "Unauthorized access: User ID mismatch." });
            }

            if (id != user.Id || id == null || user == null)
            {
                return this.BadRequest(new { message = "Bad request: ID mismatch or invalid user data." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            this._context.Entry(user).State = EntityState.Modified;

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.UserExists(id))
                {
                    return this.NotFound(new { message = "User not found." });
                }
                else
                {
                    throw;
                }
            }

            return this.NoContent();
        }

        [Authorize]
        [HttpPatch]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> PatchUser([FromBody] UserPathDTO userPathDTO)
        {
            var user = await this._context.Users.FindAsync(this._userId);
            if (user == null)
            {
                return this.NotFound(new { message = "User not found." });
            }

            if (user.IsArchived)
            {
                return this.BadRequest(new { message = "Cannot modify an archived user." });
            }

            var users = await this._context.Users
                .Where(u => u.Name == userPathDTO.Name || u.Email == userPathDTO.Email)
                .ToListAsync();

            if (users.Any(u => u.Id != this._userId))
            {
                return this.BadRequest(new { message = "User with the same name or email already exists." });
            }

            if (!string.IsNullOrEmpty(userPathDTO.Name) && ValidateUserData.isValidUsername(userPathDTO.Name))
            {
                user.Name = userPathDTO.Name;
            }

            if (!string.IsNullOrEmpty(userPathDTO.Email) && ValidateUserData.isValidEmail(userPathDTO.Email))
            {
                user.Email = userPathDTO.Email;
            }

            if (!user.IsSSO && !string.IsNullOrEmpty(userPathDTO.Password) && ValidateUserData.isValidPassword(userPathDTO.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(userPathDTO.Password);
            }

            this._context.Entry(user).State = EntityState.Modified;
            await this._context.SaveChangesAsync();

            return this.NoContent();
        }

        // DELETE: api/Users/5
        [Authorize]
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> DeleteUser(int? id)
        {
            if (this._userId != id)
            {
                return this.Unauthorized(new { message = "Unauthorized access: User ID mismatch." });
            }

            var user = await this._context.Users.FindAsync(id);
            if (user == null)
            {
                return this.NotFound(new { message = "User not found." });
            }

            user.IsArchived = true;

            await this._context.SaveChangesAsync();

            return this.NoContent();
        }

        private bool UserExists(int? id)
        {
            return this._context.Users.Any(e => e.Id == id);
        }

        [Authorize]
        [HttpPatch("profile-picture")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    return this.BadRequest("Brak danych do zapisania");
                }

                if (string.IsNullOrWhiteSpace(file.Name))
                {
                    return this.BadRequest("Nazwa mapy jest wymagana");
                }

                if (file == null || file.Length == 0)
                {
                    return this.BadRequest("Nie wybrano pliku obrazu");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return this.BadRequest($"Nieobsługiwany format pliku. Dopuszczalne rozszerzenia: {string.Join(", ", allowedExtensions)}");
                }

                const int maxFileSize = 5 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    return this.BadRequest($"Maksymalny dopuszczalny rozmiar pliku to {maxFileSize / 1024 / 1024} MB");
                }

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/images/{uniqueFileName}";
                var user = await this._context.Users.FindAsync(this._userId);
                if (user == null)
                {
                    return this.NotFound(new { message = "User not found." });
                }

                if (!string.IsNullOrEmpty(user.Image))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, user.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.Image = imageUrl;
                await this._context.SaveChangesAsync();

                return this.Ok(new { message = "Profile picture updated.", path = user.Image });
            }
            catch (Exception ex)
            {
                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Wystąpił błąd podczas przetwarzania pliku: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPatch("change-password")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            var user = await this._context.Users.FindAsync(this._userId);
            if (user == null)
            {
                return this.NotFound(new { message = "User not found." });
            }

            if (user.IsSSO)
            {
                return this.BadRequest(new { message = "Cannot change password for SSO users." });
            }

            if (!ValidateUserData.isValidPassword(changePasswordDTO.NewPassword))
            {
                return this.BadRequest(new { message = "New password does not meet the required criteria." });
            }

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.OldPassword, user.Password))
            {
                return this.BadRequest(new { message = "Current password is incorrect." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDTO.NewPassword);
            this._context.Entry(user).State = EntityState.Modified;
            await this._context.SaveChangesAsync();

            return this.Ok(new { message = "Password changed successfully." });
        }

        [Authorize]
        [HttpPatch("change-email")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDTO changeEmailDTO)
        {
            var user = await this._context.Users.FindAsync(this._userId);
            if (user == null)
            {
                return this.NotFound(new { message = "User not found." });
            }

            if (user.IsSSO)
            {
                return this.BadRequest(new { message = "Cannot change email for SSO users." });
            }

            var emailExists = await this._context.Users.AnyAsync(u => u.Email == changeEmailDTO.NewEmail && u.Id != this._userId);
            if (emailExists)
            {
                return this.BadRequest(new { message = "Email is already in use by another account." });
            }

            if (!ValidateUserData.isValidEmail(changeEmailDTO.NewEmail))
            {
                return this.BadRequest(new { message = "Invalid email format." });
            }

            user.Email = changeEmailDTO.NewEmail;
            this._context.Entry(user).State = EntityState.Modified;
            await this._context.SaveChangesAsync();

            return this.Ok(new { message = "Email changed successfully." });
        }

        [Authorize]
        [HttpGet("profile-picture")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> GetProfilePicture(string? fileName)
        {
            var user = await this._context.Users.FindAsync(this._userId);
            if (user == null)
            {
                return this.NotFound(new { message = "User or profile picture not found." });
            }

            return this.Ok(new { path = user.Image });
        }
    }
}
