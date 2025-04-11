using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clinic_Sys.Enums;

namespace Clinic_Sys.Models
{
	public class Schedule
	{
		[Key]
		public string Id { get; set; }

		[Required]
		public string DoctorId { get; set; }

		[Required]
		public WeekDay Day { get; set; } // in Enums/WeekDay

		[Required]
		public DateTime StartTime;

		[Required]
		public DateTime EndTime;

		[Required]
		public int DurationMinutes { get; set; }

		public Doctor Doctor { get; set; }

	}
}