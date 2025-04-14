using Microsoft.EntityFrameworkCore;
using Clinic_Sys.Models;

namespace Clinic_Sys.Data
{
	public class ClinicDbContext : DbContext
	{
		public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Doctor> Doctors { get; set; }
		public DbSet<Patient> Patients { get; set; }
		public DbSet<Appointment> Appointments { get; set; }
		public DbSet<MedicalRecord> MedicalRecords { get; set; }
		public DbSet<ChatMessage> ChatMessages { get; set; }
		public DbSet<ChatSession> ChatSessions { get; set; }
		public DbSet<Schedule> Schedules { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Followup appointment relationship (self-reference)
			modelBuilder.Entity<Appointment>()
				.HasOne(a => a.FollowupAppointment)
				.WithMany()
				.HasForeignKey(a => a.FollowupId)
				.OnDelete(DeleteBehavior.Restrict); // Prevent circular cascade delete

			// User-Doctor and User-Patient one-to-one
			modelBuilder.Entity<User>()
				.HasOne(u => u.doctor)
				.WithOne()
				.HasForeignKey<Doctor>(d => d.Id);

			modelBuilder.Entity<User>()
				.HasOne(u => u.patient)
				.WithOne()
				.HasForeignKey<Patient>(p => p.Id);

			// Appointment-MedicalRecord one-to-one
			modelBuilder.Entity<Appointment>()
				.HasOne(a => a.MedicalRecord)
				.WithOne(m => m.AttendedAppointment)
				.HasForeignKey<MedicalRecord>(m => m.AttendedAppointmentId);
		}
	}
}
