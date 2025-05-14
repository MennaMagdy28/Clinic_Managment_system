using Microsoft.AspNetCore.Mvc;
using Clinic_Sys.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Clinic_Sys.Data;
using Clinic_Sys.Authorization;
using Microsoft.AspNetCore.Authorization;
using Clinic_Sys.Enums;

namespace Clinic_Sys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicalRecordController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/MedicalRecord
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<MedicalRecord>>> GetMedicalRecordsByPatientId(Guid patientId)
        {
            //get appointments id from patient id
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .ToListAsync();
            var appointmentIds = appointments.Select(a => a.Id).ToList();
            var medicalRecords = await _context.MedicalRecords
                .Where(m => appointmentIds.Contains(m.AttendedAppointmentId))
                .ToListAsync();

            return medicalRecords;
        }


        // GET: api/MedicalRecord/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalRecord>> GetMedicalRecordByAppointmentId(Guid appointmentId)
        {
            var medicalRecord = await _context.MedicalRecords
                .FirstOrDefaultAsync(m => m.AttendedAppointmentId == appointmentId);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return medicalRecord;
        }

        // POST: api/MedicalRecord
        [HttpPost]
        [AuthorizeRoles(UserRole.Doctor)]
        public async Task<ActionResult<MedicalRecord>> CreateMedicalRecord(MedicalRecord medicalRecord)
        {
            var appointment = await _context.Appointments.FindAsync(medicalRecord.AttendedAppointmentId);
            if (appointment == null || appointment.Status == AppointmentStatus.Cancelled)
                return BadRequest("Cannot create medical record for an appointment that is cancelled or not found.");

            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();
            appointment.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();
            return Ok(medicalRecord);
        }


        // DELETE: api/MedicalRecord/5
        [HttpDelete("{id}")]
        [AuthorizeRoles(UserRole.Admin)]
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