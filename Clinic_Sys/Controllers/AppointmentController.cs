using Microsoft.AspNetCore.Mvc;
using Clinic_Sys.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Clinic_Sys.Data;
using Clinic_Sys.Services.Interfaces;
using Clinic_Sys.Enums;


namespace Clinic_Sys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IScheduleService _scheduleService;
        public AppointmentController(ApplicationDbContext context, IScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        // GET: api/Appointment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            return await _context.Appointments
                .ToListAsync();
        }

        [HttpGet("appointments/doctor/{doctorId}")]
        public async Task<List<Appointment>> GetAppointmentsByDoctor(Guid doctorId)
        {
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .ToListAsync();
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

        // POST: api/Appointment
        [HttpPost]
        public async Task<ActionResult<Appointment>> CreateAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
        }

        // PUT: api/Appointment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(Guid id, Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return BadRequest();
            }

            _context.Entry(appointment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(id))
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

        // DELETE: api/Appointment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppointmentExists(Guid id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        //custom methods

        [HttpGet("appointments/doctor/{doctorId}/date/{date}")]
        public async Task<ActionResult<AvailabilityResponse>> AvailableTimeSlot(Guid doctorId, DateTime date)
        {
            var appointments = await GetAppointmentsByDoctor(doctorId);
            var schedule = await _scheduleService.GetScheduleByDate(doctorId, date);

            //schedule is null if the doctor has no schedule for the day
            if (schedule == null)
                return Ok(new AvailabilityResponse {
                    Available = false,
                    Message = "Doctor has no schedule for this day"
                });

            //check if the date is within the doctor's schedule
            var dayStart = date.Date.Add(schedule.StartTime);
            var dayEnd = date.Date.Add(schedule.EndTime);
            if (date < dayStart || date > dayEnd)
                return Ok(new AvailabilityResponse {
                    Available = false,
                    Message = "Doctor is not available at this time"
                });

            var totalSlotDuration = schedule.ExaminationDurationMins + schedule.BreakDurationMins;

            //check if the time is not in the past
            if (date < DateTime.Now)
                return Ok(new AvailabilityResponse {
                    Available = false,
                    Message = "Cannot book appointments in the past"
                });

            //check if the doctor has any appointments at this time
            var hasOverlap = appointments.Any(a =>
                a.AppointmentDate.Date == date.Date &&
                a.Status == AppointmentStatus.Scheduled &&
                ((date >= a.AppointmentDate && date < a.AppointmentDate.AddMinutes(totalSlotDuration)) ||
                (date.AddMinutes(totalSlotDuration) > a.AppointmentDate && date.AddMinutes(totalSlotDuration) <= a.AppointmentDate.AddMinutes(totalSlotDuration)))
            );
            if (hasOverlap)
                return Ok(new AvailabilityResponse {
                    Available = false,
                    Message = "Doctor is not available at this time"
                });

            return Ok(new AvailabilityResponse { Available = true, Message = "Doctor is available at this time" });
        }



        [HttpPost("appointments/doctor/{doctorId}/date/{date}")]
        public async Task<ActionResult<bool>> CreateAppointment(Guid doctorId, DateTime date)
        {
            var hasOverlap = await AvailableTimeSlot(doctorId, date);
            if (!(hasOverlap.Value.Available))
            {
                return BadRequest($"{hasOverlap.Value.Message}. Please choose a different time.");
            }
            var appointment = new Appointment
            {
                DoctorId = doctorId,
                AppointmentDate = date,
                PatientId = patientId,
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
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var hasOverlap = await AvailableTimeSlot(appointment.DoctorId, newDate);
            if (!(hasOverlap.Value.Available))
            {
                return BadRequest($"{hasOverlap.Value.Message}. Please choose a different time.");
            }

            appointment.AppointmentDate = newDate;
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }



        //cancel appointment
        [HttpPut("appointments/{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
            {
                return BadRequest("Cannot cancel an appointment that is already completed or cancelled.");
            }
            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }
    }

}