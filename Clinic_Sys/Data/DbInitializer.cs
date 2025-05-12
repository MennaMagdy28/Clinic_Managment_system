using System;
using System.Threading.Tasks;
using Clinic_Sys.Models;
using Clinic_Sys.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinic_Sys.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Create roles if they don't exist
            foreach (var role in Enum.GetNames(typeof(UserRole)))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    logger.LogInformation($"Creating role: {role}");
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }

            // Create admin user if it doesn't exist
            var adminEmail = "admin@clinic.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                logger.LogInformation("Creating admin user");
                adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Admin User",
                    Role = UserRole.Admin
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    logger.LogInformation("Admin user created successfully");
                    // Add admin user to Admin role
                    var roleResult = await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Admin role assigned successfully");
                    }
                    else
                    {
                        logger.LogError($"Failed to assign admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists");
                // Verify admin role
                var isInRole = await userManager.IsInRoleAsync(adminUser, UserRole.Admin.ToString());
                if (!isInRole)
                {
                    logger.LogInformation("Admin user not in Admin role, adding role");
                    var roleResult = await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Admin role assigned successfully");
                    }
                    else
                    {
                        logger.LogError($"Failed to assign admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already has Admin role");
                }
            }
        }
    }
} 