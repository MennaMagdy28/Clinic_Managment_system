using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic_Sys.Data;
using Clinic_Sys.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Clinic_Sys.Hubs;


namespace Clinic_Sys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatMessageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatMessageController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/ChatMessage/session/{sessionId}
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessagesForSession(Guid sessionId)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "sub").Value);
            if (userId == Guid.Empty)
                return Unauthorized();

            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.Id == sessionId);

            if (session == null)
                return NotFound();

            if (session.PatientId != userId && session.DoctorId != userId)
                return Forbid();

            var messages = await _context.ChatMessages
                .Include(cm => cm.Sender)
                .Where(cm => cm.SessionId == sessionId)
                .OrderBy(cm => cm.Timestamp)
                .ToListAsync();

            return messages;
        }

        // POST: api/ChatMessage
        [HttpPost]
        public async Task<ActionResult<ChatMessage>> CreateMessage(ChatMessage message)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "sub").Value);
            if (userId == Guid.Empty)
                return Unauthorized();

            if (message.SenderId != userId)
                return Forbid();

            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.Id == message.SessionId);

            if (session == null)
                return NotFound();

            if (session.PatientId != userId && session.DoctorId != userId)
                return Forbid($"User {userId} is not part of the session {message.SessionId}");

            message.Timestamp = DateTime.UtcNow;
            message.Seen = false;

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // Send the message to all clients in the chat session group
            await _hubContext.Clients.Group(message.SessionId.ToString())
                .SendAsync("ReceiveMessage", message);

            return CreatedAtAction(nameof(GetMessagesForSession), new { sessionId = message.SessionId }, message);
        }

        // PUT: api/ChatMessage/{id}/seen
        [HttpPut("{id}/seen")]
        public async Task<IActionResult> MarkMessageAsSeen(Guid id)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "sub").Value);
            if (userId == Guid.Empty)
                return Unauthorized();

            var message = await _context.ChatMessages
                .Include(cm => cm.Session)
                .FirstOrDefaultAsync(cm => cm.Id == id);

            if (message == null)
                return NotFound();

            if (message.Session.PatientId != userId && message.Session.DoctorId != userId)
                return Forbid();

            message.Seen = true;
            await _context.SaveChangesAsync();

            // Notify all clients in the chat session that the message has been seen
            await _hubContext.Clients.Group(message.SessionId.ToString()).SendAsync("MessageSeen", id.ToString());

            return NoContent();
        }

        // DELETE: api/ChatMessage/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            var userId =  Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "sub").Value);
            if (userId == Guid.Empty)
                return Unauthorized();

            var message = await _context.ChatMessages
                .Include(cm => cm.Session)
                .FirstOrDefaultAsync(cm => cm.Id == id);

            if (message == null)
                return NotFound();

            if (message.SenderId != userId)
                return Forbid();

            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();

            // Notify all clients in the chat session that the message has been deleted
            await _hubContext.Clients.Group(message.SessionId.ToString())
                .SendAsync("MessageDeleted", id);

            return NoContent();
        }
    }
} 