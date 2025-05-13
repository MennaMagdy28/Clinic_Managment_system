using Microsoft.AspNetCore.Mvc;
using Clinic_Sys.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Clinic_Sys.Data;
using Clinic_Sys.Enums;
using Clinic_Sys.Services.Interfaces;
using Clinic_Sys.Authorization;
using Microsoft.AspNetCore.Authorization;


namespace Clinic_Sys.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IScheduleService _scheduleService;

        public ScheduleController(ApplicationDbContext context, IScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        // GET: api/Schedule/5
        [HttpGet("doctor/{doctorId}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Patient)]
        public async Task<ActionResult<List<Schedule>>> GetScheduleByDoctorId(Guid doctorId)
        {
            var schedule = await _scheduleService.GetScheduleByDoctorId(doctorId);
            return Ok(schedule);
        }

        [HttpGet("doctor/{doctorId}/day/{date}")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Patient)]
        public async Task<ActionResult<Schedule>> GetScheduleByDate(Guid doctorId, DateTime date)
        {
            var schedule = await _scheduleService.GetScheduleByDate(doctorId, date);
            if (schedule == null)
            {
                return NotFound();
            }
            return Ok(schedule);
        }


        // POST: api/Schedule
        [HttpPost]
        [AuthorizeRoles(UserRole.Admin)]
        public async Task<ActionResult<Schedule>> CreateSchedule(Schedule schedule)
        {
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return Ok(schedule);
        }

        // PUT: api/Schedule/5
        [HttpPut("{id}")]
        [AuthorizeRoles(UserRole.Admin)]
        public async Task<IActionResult> UpdateSchedule(Guid id, Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return BadRequest();
            }

            _context.Entry(schedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(id))
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

        // DELETE: api/Schedule/5
        [HttpDelete("{id}")]
        [AuthorizeRoles(UserRole.Admin)]
        public async Task<IActionResult> DeleteSchedule(Guid id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScheduleExists(Guid id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
    }
}