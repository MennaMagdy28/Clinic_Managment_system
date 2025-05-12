using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clinic_Sys.Enums;
using Microsoft.AspNetCore.Identity;

namespace Clinic_Sys.Models
{
	public class User : IdentityUser<Guid>
	{
		[Required]
		public string Name { get; set; }

		[Required]
		public UserRole Role { get; set; } // in Enums.UserRole: Admin, Doctor, Patient

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		[InverseProperty("User")]
		public Doctor? Doctor { get; set; }

		[InverseProperty("User")]
		public Patient? Patient { get; set; }
	}
}