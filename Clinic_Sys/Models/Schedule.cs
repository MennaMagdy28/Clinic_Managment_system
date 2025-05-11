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
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required, ForeignKey("Doctor")]
		public Guid DoctorId { get; set; }

		[Required]
		public WeekDay Day { get; set; } // in Enums/WeekDay

		[Required]
		public TimeSpan StartTime { get; set; }

		[Required]
		public TimeSpan EndTime { get; set; }

		[Required]
		public int BreakDurationMins { get; set; } // in minutes

		[Required]
		public int ExaminationDurationMins { get; set; }

		public Doctor Doctor { get; set; }
	}
}