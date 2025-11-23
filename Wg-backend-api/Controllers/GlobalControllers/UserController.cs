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

        // get user data
        [Authorize]
        [HttpGet("{id}")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<ActionResult<User>> GetUser(int? id)
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

            return this.Ok(user);
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

            if (!user.IsSSO && !string.IsNullOrEmpty(userPathDTO.Password))
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
        [HttpPost("{id}/profile-picture")]
        [ServiceFilter(typeof(UserIdActionFilter))]
        public async Task<IActionResult> UploadProfilePicture(int? id, IFormFile file)
        {
            if (this._userId != id)
            {
                return this.Unauthorized(new { message = "Unauthorized access: User ID mismatch." });
            }

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

                const int maxFileSize = 20 * 1024 * 1024;
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
                var user = await this._context.Users.FindAsync(id);
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
    }
}
