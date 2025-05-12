using Microsoft.AspNetCore.Mvc;
using Clinic_Sys.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Clinic_Sys.Data;
using Clinic_Sys.Authorization;
using Microsoft.AspNetCore.Authorization;

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

        // GET: api/ChatSession
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatSession>>> GetChatSessions()
        {
            return await _context.ChatSessions
                .ToListAsync();
        }

        // GET: api/ChatSession/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatSession>> GetChatSession(Guid id)
        {
            var chatSession = await _context.ChatSessions
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chatSession == null)
            {
                return NotFound();
            }

            return chatSession;
        }

        // POST: api/ChatSession
        [HttpPost]
        public async Task<ActionResult<ChatSession>> CreateChatSession(ChatSession chatSession)
        {
            _context.ChatSessions.Add(chatSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChatSession), new { id = chatSession.Id }, chatSession);
        }
        

        // DELETE: api/ChatSession/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatSession(Guid id)
        {
            var chatSession = await _context.ChatSessions.FindAsync(id);
            if (chatSession == null)
            {
                return NotFound();
            }

            _context.ChatSessions.Remove(chatSession);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChatSessionExists(Guid id)
        {
            return _context.ChatSessions.Any(e => e.Id == id);
        }
    }
} 