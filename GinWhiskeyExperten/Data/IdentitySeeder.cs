using Microsoft.AspNetCore.Identity;
using GinWhiskeyExperten.Models;

namespace GinWhiskeyExperten.Data
{
    public static class IdentitySeeder
    {
        public const string AdminRole = "Admin";

        // Reads the admin credentials from configuration (User Secrets locally, App Service
        // settings in Azure) at runtime rather than baking a password hash into a migration file -
        // idempotent, safe to run on every app start.
        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
            }

            var adminEmail = configuration["AdminUser:Email"];
            var adminPassword = configuration["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("AdminUser:Email / AdminUser:Password not configured - skipping admin seed. " +
                    "Set these via User Secrets locally or App Service settings in Azure.");
                return;
            }

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AdminRole);
                    logger.LogInformation("Seeded Admin user {Email}.", adminEmail);
                }
                else
                {
                    logger.LogError("Failed to seed Admin user: {Errors}",
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
            else if (!await userManager.IsInRoleAsync(existingAdmin, AdminRole))
            {
                await userManager.AddToRoleAsync(existingAdmin, AdminRole);
            }
        }
    }
}
