using Microsoft.AspNetCore.Mvc;
using Clinic_Sys.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Clinic_Sys.Data;

namespace Clinic_Sys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MedicalRecord
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicalRecord>>> GetMedicalRecords()
        {
            return await _context.MedicalRecords
                .ToListAsync();
        }

        // GET: api/MedicalRecord/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalRecord>> GetMedicalRecord(Guid id)
        {
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.AttendedAppointment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return medicalRecord;
        }

        // POST: api/MedicalRecord
        [HttpPost]
        public async Task<ActionResult<MedicalRecord>> CreateMedicalRecord(MedicalRecord medicalRecord)
        {
            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMedicalRecord), new { id = medicalRecord.Id }, medicalRecord);
        }

        // PUT: api/MedicalRecord/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedicalRecord(Guid id, MedicalRecord medicalRecord)
        {
            if (id != medicalRecord.Id)
            {
                return BadRequest();
            }

            _context.Entry(medicalRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MedicalRecordExists(id))
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

        // DELETE: api/MedicalRecord/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalRecord(Guid id)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null)
            {
                return NotFound();
            }

            _context.MedicalRecords.Remove(medicalRecord);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MedicalRecordExists(Guid id)
        {
            return _context.MedicalRecords.Any(e => e.Id == id);
        }
    }
} 