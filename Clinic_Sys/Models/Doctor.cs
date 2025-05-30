using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clinic_Sys.Models;
using System.Text.Json.Serialization;
namespace Clinic_Sys.Models
{
	public class Doctor
	{
		[Key]
		public Guid Id { get; set; }
		[Required]
		public string Specialization { get; set; }
		[Required]
		public string LicenseNumber { get; set; }
		[Required]
		public int RoomNumber { get; set; }

		[JsonIgnore]
		public User? User { get; set; }
		public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
		public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
		public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
	}
}