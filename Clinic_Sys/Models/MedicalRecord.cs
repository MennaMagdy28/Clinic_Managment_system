using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clinic_Sys.Models;

namespace Clinic_Sys.Models
{
	public class MedicalRecord
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public Guid AttendedAppointmentId { get; set; }

		public string Diagnosis { get; set; }

		public string Prescription { get; set; }

		[ForeignKey("AttendedAppointmentId")]
		public Appointment AttendedAppointment { get; set; }

	}
}