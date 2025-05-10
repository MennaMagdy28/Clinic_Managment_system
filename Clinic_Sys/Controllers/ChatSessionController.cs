using Microsoft.AspNetCore.Mvc;
using Clinic_Sys.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Clinic_Sys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                .Include(c => c.Patient)
                .Include(c => c.Doctor)
                .Include(c => c.Messages)
                .ToListAsync();
        }

        // GET: api/ChatSession/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatSession>> GetChatSession(string id)
        {
            var chatSession = await _context.ChatSessions
                .Include(c => c.Patient)
                .Include(c => c.Doctor)
                .Include(c => c.Messages)
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

        // PUT: api/ChatSession/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChatSession(string id, ChatSession chatSession)
        {
            if (id != chatSession.Id)
            {
                return BadRequest();
            }

            _context.Entry(chatSession).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatSessionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ChatSession/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatSession(string id)
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

        private bool ChatSessionExists(string id)
        {
            return _context.ChatSessions.Any(e => e.Id == id);
        }
    }
} 