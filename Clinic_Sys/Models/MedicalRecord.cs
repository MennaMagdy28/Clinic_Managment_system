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
		public string Id { get; set; }

		[ForeignKey("Appointment"), Required]
		public string AttendedAppointmentId { get; set; }

		public string Diagnosis { get; set; }

		public string Prescription { get; set; }


		public Appointment AttendedAppointment { get; set; }

	}
}