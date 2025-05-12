using Microsoft.EntityFrameworkCore;
using Clinic_Sys.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace Clinic_Sys.Data
{
	public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

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
				.WithMany(a => a.FollowupAppointments)
				.HasForeignKey(a => a.FollowupId)
				.OnDelete(DeleteBehavior.Restrict);

			// User-Doctor/Patient one-to-one
			modelBuilder.Entity<User>()
				.HasOne(u => u.Doctor)
				.WithOne(d => d.User)
				.HasForeignKey<Doctor>(d => d.Id)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<User>()
				.HasOne(u => u.Patient)
				.WithOne(p => p.User)
				.HasForeignKey<Patient>(p => p.Id)
				.OnDelete(DeleteBehavior.Cascade);

			// Appointment-MedicalRecord one-to-one with cascade delete
			modelBuilder.Entity<Appointment>()
				.HasOne(a => a.MedicalRecord)
				.WithOne(m => m.AttendedAppointment)
				.HasForeignKey<MedicalRecord>(m => m.AttendedAppointmentId)
				.OnDelete(DeleteBehavior.Cascade);

			// Doctor-Schedule relationship
			modelBuilder.Entity<Schedule>()
				.HasOne(s => s.Doctor)
				.WithMany(d => d.Schedules)
				.HasForeignKey(s => s.DoctorId)
				.OnDelete(DeleteBehavior.Cascade);

			// ChatSession relationships
			modelBuilder.Entity<ChatSession>()
				.HasOne(cs => cs.Doctor)
				.WithMany(d => d.ChatSessions)
				.HasForeignKey(cs => cs.DoctorId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ChatSession>()
				.HasOne(cs => cs.Patient)
				.WithMany(p => p.ChatSessions)
				.HasForeignKey(cs => cs.PatientId)
				.OnDelete(DeleteBehavior.Restrict);

			// ChatMessage relationships
			modelBuilder.Entity<ChatMessage>()
				.HasOne(cm => cm.Session)
				.WithMany(cs => cs.Messages)
				.HasForeignKey(cm => cm.SessionId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ChatMessage>()
				.HasOne(cm => cm.Sender)
				.WithMany()
				.HasForeignKey(cm => cm.SenderId)
				.OnDelete(DeleteBehavior.Restrict);

			// Appointment relationships
			modelBuilder.Entity<Appointment>()
				.HasOne(a => a.Doctor)
				.WithMany(d => d.Appointments)
				.HasForeignKey(a => a.DoctorId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Appointment>()
				.HasOne(a => a.Patient)
				.WithMany(p => p.Appointments)
				.HasForeignKey(a => a.PatientId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
