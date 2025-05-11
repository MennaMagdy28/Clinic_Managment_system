using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic_Sys.Models
{
	public class ChatMessage
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public Guid SessionId { get; set; }

		[Required]
		public Guid SenderId { get; set; }

		[Required]
		public string Message { get; set; }

		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		public bool Seen { get; set; }

		[ForeignKey("SessionId")]
		public ChatSession Session { get; set; }

		[ForeignKey("SenderId")]
		public User Sender { get; set; }
	}
}