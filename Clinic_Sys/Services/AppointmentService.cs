using Microsoft.AspNetCore.Mvc;
using Clinic_Sys.Models;
using Clinic_Sys.Models.DTOs;
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

            // Sort by AppointmentDate descending (most recent first)
            query = query.OrderByDescending(a => a.AppointmentDate);

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

        public async Task<AvailabilityResponse> AvailableTimeSlot(Guid doctorId, DateTime date){
            var appointments = await GetFilteredAppointments(doctorId, date);
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






        //Link appointment with follow up appointment
        public async Task<Appointment> LinkAppointmentWithFollowup(Guid AppointmentId, Guid FollowupId){
            var appointment = await _context.Appointments.FindAsync(AppointmentId);
            if (appointment == null)
                return null;
            appointment.FollowupId = FollowupId;
            await _context.SaveChangesAsync();
            return appointment;
        }

    }
}