using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clinic_Sys.Enums;
using System.Text.Json.Serialization;
namespace Clinic_Sys.Models
{
	public class Patient
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Gender Gender { get; set; } // in Enums.Gender: Male, Female

		[Required]
		public DateTime BirthDate { get; set; }

		public string ChronicDisease { get; set; }

		[JsonIgnore]
		public User? User { get; set; }
		public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
		public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

	}
}