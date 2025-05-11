using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clinic_Sys.Enums;

namespace Clinic_Sys.Models
{
	public class User
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public string Name { get; set; }

		[Required, EmailAddress]
		public string Email { get; set; }

		[Required, DataType(DataType.Password)]
		public string Password { get; set; }

		[Required]
		public UserRole Role { get; set; } // in Enums.UserRole: Admin, Doctor, Patient

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


		[InverseProperty("User")]
		public Doctor Doctor { get; set; }

		[InverseProperty("User")]
		public Patient Patient { get; set; }
	}
}