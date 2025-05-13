using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clinic_Sys.Models;
using System.Text.Json.Serialization;
namespace Clinic_Sys.Models
{
	public class MedicalRecord
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required, ForeignKey("Appointment")]
		public Guid AttendedAppointmentId { get; set; }

		public string Diagnosis { get; set; }

		public string Prescription { get; set; }

		[JsonIgnore]
		public Appointment? AttendedAppointment { get; set; }

	}
}