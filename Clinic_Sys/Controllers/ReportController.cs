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
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IScheduleService _scheduleService;
        private readonly IAppointmentService _appointmentService;
        public ReportController(ApplicationDbContext context, IScheduleService scheduleService, IAppointmentService appointmentService)
        {
            _context = context;
            _scheduleService = scheduleService;
            _appointmentService = appointmentService;
        }

        [HttpGet("appointments/grouped")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Doctor)]
        public async Task<IActionResult> GetAppointmentsGroupedByStatus(Guid? doctorId, DateTime? date)
        {
            // If the user is a doctor, force doctorId from claims
            if (User.IsInRole(UserRole.Doctor.ToString()))
            {
                doctorId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            }
            var appointments = await _appointmentService.GroupAndSortAppointments(doctorId, date);
            return Ok(appointments);
        }

        [HttpGet("appointments/filtered")]
        [AuthorizeRoles(UserRole.Admin, UserRole.Doctor)]
        public async Task<IActionResult> GetFilteredAppointments(Guid? doctorId, DateTime? date)
        {
            // If the user is a doctor, force doctorId from claims
            if (User.IsInRole(UserRole.Doctor.ToString()))
            {
                doctorId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            }
            var appointments = await _appointmentService.GetFilteredAppointments(doctorId, date);
            return Ok(appointments);
        }
    }
}
