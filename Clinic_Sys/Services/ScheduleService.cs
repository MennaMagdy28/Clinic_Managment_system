using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clinic_Sys.Data;
using Clinic_Sys.Models;
using Clinic_Sys.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Clinic_Sys.Enums;
using Clinic_Sys.Data;

namespace Clinic_Sys.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ApplicationDbContext _context;

        public ScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Schedule>> GetScheduleByDoctorId(Guid doctorId)
        {
            return await _context.Schedules
                .Where(s => s.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<Schedule?> GetScheduleByDate(Guid doctorId, DateTime date)
        {
            return await _context.Schedules
            .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.Day == (WeekDay)date.DayOfWeek);
        }
    }
}
