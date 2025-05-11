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

		public Guid PatientId { get; set; }

		public Guid DoctorId { get; set; }

		public DateTime CreatedAt { get; set; }

		[ForeignKey("PatientId")]
		public Patient Patient { get; set; }

		[ForeignKey("DoctorId")]
		public Doctor Doctor { get; set; }

		[InverseProperty("Session")]
		public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
	}
}