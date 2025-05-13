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


        // DELETE: api/Appointment/5
        [HttpDelete("{id}")]
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

        [HttpGet("appointments/doctor/{doctorId}/date/{date}")]
        public async Task<AvailabilityResponse> AvailableTimeSlot(Guid doctorId, DateTime date)
        {
            var appointments = await _appointmentService.GetFilteredAppointments(doctorId, date);
            var schedule = await _scheduleService.GetScheduleByDate(doctorId, date);

            //schedule is null if the doctor has no schedule for the day
            if (schedule == null)
                return (new AvailabilityResponse {
                    Available = false,
                    Message = "Doctor has no schedule for this day"
                });

            //check if the date is within the doctor's schedule
            var dayStart = date.Date.Add(schedule.StartTime);
            var dayEnd = date.Date.Add(schedule.EndTime);
            if (date < dayStart || date > dayEnd)
                return (new AvailabilityResponse {
                    Available = false,
                    Message = "Doctor is not available at this time"
                });

            var totalSlotDuration = schedule.ExaminationDurationMins + schedule.BreakDurationMins;

            //check if the time is not in the past
            if (date < DateTime.Now)
                return (new AvailabilityResponse {
                    Available = false,
                    Message = "Cannot book appointments in the past"
                });

            //check if the doctor has any appointments at this time
            var hasOverlap = appointments.Any(a =>
                a.AppointmentDate.Date == date.Date &&
                a.Status == AppointmentStatus.Scheduled &&
                ((date >= a.AppointmentDate && date < a.AppointmentDate.AddMinutes(totalSlotDuration)) ||
                (date.AddMinutes(totalSlotDuration) > a.AppointmentDate &&
                 date.AddMinutes(totalSlotDuration) <= a.AppointmentDate.AddMinutes(totalSlotDuration)))
            );
            if (hasOverlap)
                return (new AvailabilityResponse {
                    Available = false,
                    Message = "Doctor is not available at this time"
                });

            return (new AvailabilityResponse { Available = true, Message = "Doctor is available at this time" });
        }



        [HttpPost("appointments/doctor/{doctorId}/date/{date}")]
        [AuthorizeRoles(UserRole.Patient)]
        public async Task<ActionResult<bool>> CreateAppointment(Guid doctorId, DateTime date)
        {
            // Get current user ID from JWT
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "sub");
            if (userIdClaim == null)
            {
                return Unauthorized("User not found in token");
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var hasOverlap = await AvailableTimeSlot(doctorId, date);
            if (!(hasOverlap.Available))
            {
                return BadRequest($"{hasOverlap.Message}. Please choose a different time.");
            }
            var appointment = new Appointment
            {
                DoctorId = doctorId,
                AppointmentDate = date,
                PatientId = userId,
                Status = AppointmentStatus.Scheduled
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }




        //reschedule appointment
        [HttpPut("appointments/{id}")]
        public async Task<IActionResult> RescheduleAppointment(Guid id, DateTime newDate)
        {
            var appointment = await GetAppointment(id);

            var hasOverlap = await AvailableTimeSlot(appointment.Value.DoctorId, newDate);
            if (hasOverlap == null || !(hasOverlap.Available))
            {
                return BadRequest($"{hasOverlap.Message}. Please choose a different time.");
            }

            appointment.Value.AppointmentDate = newDate;
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }



        //cancel appointment
        [HttpPut("appointments/{id}/cancel")]
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
    }

}