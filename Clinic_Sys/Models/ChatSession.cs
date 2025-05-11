using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic_Sys.Models
{
	public class ChatSession
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required, ForeignKey("Patient")]
		public Guid PatientId { get; set; }

		[Required, ForeignKey("Doctor")]
		public Guid DoctorId { get; set; }

		public DateTime CreatedAt { get; set; }

		public Patient? Patient { get; set; }

		public Doctor? Doctor { get; set; }

		[InverseProperty("Session")]
		public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
	}
}