using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic_Sys.Models
{
	public class ChatSession
	{
		[Key]
		public string Id { get; set; }

		public string PatientId { get; set; }

		public string DoctorId { get; set; }

		public DateTime CreatedAt { get; set; }

		public Patient Patient { get; set; }
		public Doctor Doctor { get; set; }
		public ICollection<ChatMessage> Messages { get; set; }

	}
}