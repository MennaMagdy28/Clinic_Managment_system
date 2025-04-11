using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic_Sys.Models
{
	public class ChatMessage
	{
		[Key]
		public string Id { get; set; }

		public string SessionId { get; set; }

		public string SenderId { get; set; }

		public string Message { get; set; }

		public DateTime Timestamp { get; set; }

		public bool Seen { get; set; }

		public ChatSession Session { get; set; }
		public User Sender { get; set; }
	}
}