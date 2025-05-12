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
        private readonly IScheduleService _scheduleService;

        public AppointmentService(ApplicationDbContext context, IScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
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

        public async Task<List<TimeSlot>> GetAvailableTimeSlots(Guid doctorId, DateTime date)
        {
            var schedule = await _scheduleService.GetScheduleByDate(doctorId, date);
            if (schedule == null)
                return new List<TimeSlot>();

            var appointments = await GetFilteredAppointments(doctorId, date);
            int totalSlotDuration = schedule.ExaminationDurationMins + schedule.BreakDurationMins;
            var dayStart = date.Date.Add(schedule.StartTime);
            var dayEnd = date.Date.Add(schedule.EndTime);

            var occupiedSlots = appointments
            .Where(a => a.Status == AppointmentStatus.Scheduled)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new TimeSlot{
                StartTime = a.AppointmentDate,
                EndTime = a.AppointmentDate.AddMinutes(totalSlotDuration)
            })
            .ToList();

            var availableSlots = new List<TimeSlot>();
            var currentSlot = dayStart;

            foreach (var slot in occupiedSlots)
            {
                if (currentSlot < slot.StartTime)
                {
                    var availableDuration = (slot.StartTime - currentSlot).TotalMinutes;

                    if (availableDuration >= schedule.ExaminationDurationMins)
                    {
                        availableSlots.Add(new TimeSlot
                        {
                            StartTime = currentSlot,
                            EndTime = slot.StartTime
                        });
                    }
                }
                currentSlot = slot.EndTime > currentSlot ? slot.EndTime : currentSlot;
            }

            if (currentSlot < dayEnd)
            {
                var availableDuration = (dayEnd - currentSlot).TotalMinutes;

                if (availableDuration >= schedule.ExaminationDurationMins)
                {
                    availableSlots.Add(new TimeSlot
                    {
                        StartTime = currentSlot,
                        EndTime = dayEnd
                    });
                }
            }

            return availableSlots;
        }
    }
}