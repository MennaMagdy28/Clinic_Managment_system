using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clinic_Sys.Enums;

namespace Clinic_Sys.Models
{
	public class Appointment
	{
		[Key]
		public string Id { get; set; }

		[Required]
		public string PatientId { get; set; }

		[Required]
		public string DoctorId { get; set; }

		[Required]
		public DateTime AppointmentDate { get; set; }

		[Required]
		public AppointmentStatus Status { get; set; } // in Enums.AppointmentStatus: Scheduled, Completed, Canceled

		public string? FollowupId { get; set; }


		public Patient Patient { get; set; }
		public Doctor Doctor { get; set; }
		public Appointment FollowupAppointment { get; set; }
		public MedicalRecord MedicalRecord { get; set; }
	}
}