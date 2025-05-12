using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clinic_Sys.Models;
using Clinic_Sys.Enums;

namespace Clinic_Sys.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<List<Schedule>> GetScheduleByDoctorId(Guid doctorId);
        Task<Schedule?> GetScheduleByDate(Guid doctorId, DateTime date);
    }
}
