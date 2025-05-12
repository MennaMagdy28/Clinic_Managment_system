using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clinic_Sys.Models;
using Clinic_Sys.Enums;

namespace Clinic_Sys.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<Dictionary<AppointmentStatus, List<Appointment>>> GroupAndSortAppointments(Guid? doctorId, DateTime? date);
        Task<List<Appointment>> GetFilteredAppointments(Guid? doctorId, DateTime? date);
    }
}
