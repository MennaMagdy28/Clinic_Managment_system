using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic_Sys.Data;
using Clinic_Sys.Models;
using Clinic_Sys.Enums;
using Clinic_Sys.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Clinic_Sys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatSessionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatSessionController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            // Try different claim types that might contain the user ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??  // Standard claim type
                        User.FindFirst("nameid")?.Value ??                    // JWT specific
                        User.FindFirst("sub")?.Value;                         // OAuth specific

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            return userId;
        }

        // GET: api/ChatSession
        [HttpGet]
        [AuthorizeRoles(UserRole.Doctor, UserRole.Patient)]
        public async Task<ActionResult<IEnumerable<ChatSession>>> GetChatSessions()
        {
            try
            {
                var userId = GetUserId();
                var sessions = await _context.ChatSessions
                    .Include(cs => cs.Patient)
                    .Include(cs => cs.Doctor)
                    .Where(cs => cs.PatientId.ToString() == userId || cs.DoctorId.ToString() == userId)
                    .ToListAsync();

                return sessions;
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        // GET: api/ChatSession/5
        [HttpGet("{id}")]
        [AuthorizeRoles(UserRole.Doctor, UserRole.Patient)]
        public async Task<ActionResult<ChatSession>> GetChatSession(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var chatSession = await _context.ChatSessions
                    .Include(cs => cs.Patient)
                    .Include(cs => cs.Doctor)
                    .Include(cs => cs.Messages)
                    .FirstOrDefaultAsync(cs => cs.Id == id);

                if (chatSession == null)
                    return NotFound();

                if (chatSession.PatientId.ToString() != userId && chatSession.DoctorId.ToString() != userId)
                    return Forbid();

                return chatSession;
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        // POST: api/ChatSession
        [HttpPost]
        [AuthorizeRoles(UserRole.Doctor, UserRole.Patient)]
        public async Task<ActionResult<ChatSession>> CreateChatSession(ChatSession chatSession)
        {
            try
            {
                var userId = GetUserId();

                // Get the user and their role
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (user == null)
                    return Unauthorized();

                // Verify that the user is either the patient or the doctor based on their role
                if (user.Role == UserRole.Patient && chatSession.PatientId.ToString() != userId)
                {
                    return Forbid();
                }
                else if (user.Role == UserRole.Doctor && chatSession.DoctorId.ToString() != userId)
                {
                    return Forbid();
                }

                // Verify that both patient and doctor exist
                var patientExists = await _context.Patients.AnyAsync(p => p.Id == chatSession.PatientId);
                var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == chatSession.DoctorId);

                if (!patientExists || !doctorExists)
                {
                    return BadRequest("Invalid patient or doctor ID");
                }

                // Check if a chat session already exists between these users
                var existingSession = await _context.ChatSessions
                    .FirstOrDefaultAsync(cs => 
                        (cs.PatientId == chatSession.PatientId && cs.DoctorId == chatSession.DoctorId) ||
                        (cs.PatientId == chatSession.DoctorId && cs.DoctorId == chatSession.PatientId));

                if (existingSession != null)
                {
                    return BadRequest("A chat session already exists between these users");
                }

                chatSession.CreatedAt = DateTime.UtcNow;
                _context.ChatSessions.Add(chatSession);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetChatSession), new { id = chatSession.Id }, chatSession);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        // DELETE: api/ChatSession/5
        [HttpDelete("{id}")]
        [AuthorizeRoles(UserRole.Doctor, UserRole.Patient)]
        public async Task<IActionResult> DeleteChatSession(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var chatSession = await _context.ChatSessions.FindAsync(id);
                if (chatSession == null)
                    return NotFound();

                if (chatSession.PatientId.ToString() != userId && chatSession.DoctorId.ToString() != userId)
                    return Forbid();

                _context.ChatSessions.Remove(chatSession);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
} 