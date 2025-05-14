using Microsoft.AspNetCore.Mvc;
using Clinic_Sys.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Clinic_Sys.Data;
using Microsoft.AspNetCore.Identity;
using Clinic_Sys.Authorization;
using Clinic_Sys.Enums;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Clinic_Sys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        [AuthorizeRoles(UserRole.Admin)] // Only admins can list all users
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Users can only access their own profile unless they're an admin
            if (currentUser.Id != id && currentUser.Role != UserRole.Admin)
            {
                return Forbid();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserUpdateModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Users can only update their own profile unless they're an admin
            if (currentUser.Id != id && currentUser.Role != UserRole.Admin)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.Name = model.Name;

            // Handle email update
            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                // Check if email is already taken
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email is already taken" });
                }

                // Update email
                user.Email = model.Email;
                user.UserName = model.Email; // Username should match email
            }

            // Only admins can change roles
            if (currentUser.Role == UserRole.Admin && model.Role.HasValue)
            {
                user.Role = model.Role.Value;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        [AuthorizeRoles(UserRole.Admin)] // Only admins can delete users
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }

    public class UserUpdateModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public UserRole? Role { get; set; }
    }
}