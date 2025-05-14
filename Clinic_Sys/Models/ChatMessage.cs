using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Clinic_Sys.Models
{
	public class ChatMessage
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required, ForeignKey("ChatSession")]
		public Guid SessionId { get; set; }

		[Required, ForeignKey("User")]
		public Guid SenderId { get; set; }

		[Required]
		public string Message { get; set; }

		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		public bool Seen { get; set; }

		[JsonIgnore]
		public ChatSession? Session { get; set; }

		[JsonIgnore]
		public User? Sender { get; set; }
	}
}