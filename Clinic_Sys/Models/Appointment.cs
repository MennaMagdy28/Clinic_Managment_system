using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clinic_Sys.Enums;
using System.Text.Json.Serialization;

namespace Clinic_Sys.Models
{
	public class Appointment
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required, ForeignKey("Patient")]
		public Guid PatientId { get; set; }

		[Required, ForeignKey("Doctor")]
		public Guid DoctorId { get; set; }

		[Required]
		public DateTime AppointmentDate { get; set; }

		[Required]
		public AppointmentStatus Status { get; set; } // in Enums.AppointmentStatus: Scheduled, Completed, Canceled

		[ForeignKey("Appointment")]
		public Guid? FollowupId { get; set; }

		public Patient? Patient { get; set; }
		public Doctor? Doctor { get; set; }
		[JsonIgnore]
		public Appointment? FollowupAppointment { get; set; }
		[JsonIgnore]
		public ICollection<Appointment>? FollowupAppointments { get; set; }
		public MedicalRecord? MedicalRecord { get; set; }
	}
}