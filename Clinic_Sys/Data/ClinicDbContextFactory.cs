using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Clinic_Sys.Data
{
	public class ClinicDbContextFactory : IDesignTimeDbContextFactory<ClinicDbContext>
	{
		public ClinicDbContext CreateDbContext(string[] args)
		{

			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			var connectionString = configuration.GetConnectionString("DefaultConnection");

			var optionsBuilder = new DbContextOptionsBuilder<ClinicDbContext>();
			optionsBuilder.UseNpgsql(connectionString);

			return new ClinicDbContext(optionsBuilder.Options);
		}
	}
}
