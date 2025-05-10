using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
    }
}


