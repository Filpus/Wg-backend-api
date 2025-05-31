using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wg_backend_api.Auth;
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

        public UserController(GlobalDbContext context)
        {
            _context = context;
        }

        // get user data
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (!int.TryParse(userId, out int parsedUserId) || parsedUserId != id)
            {
                return Unauthorized(new { message = "Unauthorized access: User ID mismatch." });
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user); 
        }

        // Edit user data 
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int? id, User user)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int parsedUserId) || parsedUserId != id)
            {
                return Unauthorized(new { message = "Unauthorized access: User ID mismatch." });
            }

            if (id != user.Id || id == null || user == null)
            {
                return BadRequest(new { message = "Bad request: ID mismatch or invalid user data." });
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(new { message = "User not found." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // Register a new user
        [HttpPost]  
        public async Task<ActionResult<User>> PostUser(User user)
        {
            user.Id = null;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
            //return CreatedAtAction("GetUser", new { id = 1 }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int parsedUserId) || parsedUserId != id)
            {
                return Unauthorized(new { message = "Unauthorized access: User ID mismatch." });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            user.IsArchived = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int? id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpPost("{id}/profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(int? id, IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int parsedUserId) || parsedUserId != id)
            {
                return Unauthorized(new { message = "Unauthorized access: User ID mismatch." });
            }

            try
            {
                if (file == null)
                    return BadRequest("Brak danych do zapisania");

                if (string.IsNullOrWhiteSpace(file.Name))
                    return BadRequest("Nazwa mapy jest wymagana");

                if (file == null || file.Length == 0)
                    return BadRequest("Nie wybrano pliku obrazu");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest($"Nieobsługiwany format pliku. Dopuszczalne rozszerzenia: {string.Join(", ", allowedExtensions)}");

                const int maxFileSize = 20 * 1024 * 1024;
                if (file.Length > maxFileSize)
                    return BadRequest($"Maksymalny dopuszczalny rozmiar pliku to {maxFileSize / 1024 / 1024} MB");

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                
                var imageUrl = $"/images/{uniqueFileName}";
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
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
                await _context.SaveChangesAsync();

                return Ok(new { message = "Profile picture updated.", path = user.Image });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Wystąpił błąd podczas przetwarzania pliku: {ex.Message}"
                );
            }
        }
    }
}


