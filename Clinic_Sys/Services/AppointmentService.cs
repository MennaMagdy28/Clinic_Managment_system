using Clinic_Sys.Models;
using Clinic_Sys.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Clinic_Sys.Enums;
using Clinic_Sys.Data;

namespace Clinic_Sys.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Appointment>> GetFilteredAppointments(
            Guid? doctorId = null, DateTime? date = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (doctorId != null)
            {
                query = query.Where(a => a.DoctorId == doctorId);
            }

            if (date != null)
            {
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
            }

            return await query.ToListAsync();
        }
        
        public async Task<Dictionary<AppointmentStatus, List<Appointment>>> GroupAndSortAppointments(
            Guid? doctorId = null, DateTime? date = null)
        {
            var query = _context.Appointments.AsQueryable();

            var appointments = await GetFilteredAppointments(doctorId, date);

            var groupedAppointments = appointments.GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Key switch
                {
                    AppointmentStatus.Scheduled => g.OrderBy(a => a.AppointmentDate).ToList(),
                    AppointmentStatus.Completed => g.OrderByDescending(a => a.AppointmentDate).ToList(),
                    AppointmentStatus.Cancelled => g.OrderBy(a => a.AppointmentDate).ToList(),
                    _ => new List<Appointment>()
                });

            return groupedAppointments;
        }
    }
}