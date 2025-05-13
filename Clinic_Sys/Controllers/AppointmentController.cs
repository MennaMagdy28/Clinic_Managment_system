using Microsoft.AspNetCore.Mvc;
using Clinic_Sys.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Clinic_Sys.Data;
using Clinic_Sys.Services.Interfaces;
using Clinic_Sys.Enums;
using Clinic_Sys.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace Clinic_Sys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IScheduleService _scheduleService;
        private readonly IAppointmentService _appointmentService;
        public AppointmentController(ApplicationDbContext context, IScheduleService scheduleService, IAppointmentService appointmentService)
        {
            _context = context;
            _scheduleService = scheduleService;
            _appointmentService = appointmentService;
        }

        // GET: api/Appointment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(Guid id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return appointment;
        }


        [HttpGet("appointments/patient/{patientId}")]
        public async Task<IActionResult> GetAppointmentsByPatientId(Guid patientId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .ToListAsync();
            return Ok(appointments);
        }


        // DELETE: api/Appointment/5
        [HttpDelete("{id}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Patient)]
        public async Task<IActionResult> DeleteAppointment(Guid id)
        {
            var appointment = await GetAppointment(id);

            _context.Appointments.Remove(appointment.Value);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppointmentExists(Guid id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        //custom methods

        [HttpPost("appointments/doctor/{doctorId}/date/{date}")]
        [AuthorizeRoles(UserRole.Patient)]
        public async Task<ActionResult<bool>> CreateAppointment(Guid doctorId, DateTime date)
        {
            var hasOverlap = await _appointmentService.AvailableTimeSlot(doctorId, date);
            if (!(hasOverlap.Available))
            {
                return BadRequest($"{hasOverlap.Message}. Please choose a different time.");
            }
            var appointment = new Appointment
            {
                DoctorId = doctorId,
                AppointmentDate = date,
                PatientId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
                Status = AppointmentStatus.Scheduled
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }




        //reschedule appointment
        [HttpPut("appointments/{id}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Patient)]
        public async Task<IActionResult> RescheduleAppointment(Guid id, DateTime newDate)
        {
            var appointment = await GetAppointment(id);

            var hasOverlap = await _appointmentService.AvailableTimeSlot(appointment.Value.DoctorId, newDate);
            if (hasOverlap == null || !(hasOverlap.Available))
            {
                return BadRequest($"{hasOverlap.Message}. Please choose a different time.");
            }

            appointment.Value.AppointmentDate = newDate;
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }


        //Booking follow up appointment
        [HttpPost("appointments/followup")]
        [AuthorizeRoles(UserRole.Admin)]
        public async Task<IActionResult> BookFollowUpAppointment(Guid AppointmentId, DateTime date)
        {
            var appointment = await GetAppointment(AppointmentId);
            var followup = await _appointmentService.BookFollowUpAppointment(AppointmentId, date);
            var linked = await _appointmentService.LinkAppointmentWithFollowup(AppointmentId, followup.Id);
            return Ok(linked);
        }



        //cancel appointment
        [HttpPut("appointments/{id}/cancel")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Patient)]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            var appointment = await GetAppointment(id);

            if (appointment.Value.Status == AppointmentStatus.Completed || appointment.Value.Status == AppointmentStatus.Cancelled)
            {
                return BadRequest("Cannot cancel an appointment that is already completed or cancelled.");
            }
            appointment.Value.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }


        [HttpGet("appointments/free-slots")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Patient)]
        public async Task<IActionResult> GetFreeSlots(Guid doctorId, DateTime date)
        {
            var freeSlots = await _appointmentService.GetFreeSlots(doctorId, date);
            return Ok(freeSlots);
        }


    }
}