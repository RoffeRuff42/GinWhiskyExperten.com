using GinWhiskeyExperten.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GinWhiskeyExperten.Tests.TestDoubles
{
    // Boots the real app pipeline (Program.cs) against an isolated EF Core InMemory database
    // instead of SQL Server, with test-only Jwt/AdminUser configuration so the app's normal
    // startup seeding routine creates a known Admin account to log in with.
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public const string AdminEmail = "admin@test.local";
        public const string AdminPassword = "TestAdmin123!";

        private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "integration-test-signing-key-needs-to-be-long-enough-for-hmacsha256",
                    ["Jwt:Issuer"] = "GinWhiskeyExperten.Tests",
                    ["Jwt:Audience"] = "GinWhiskeyExperten.Tests",
                    ["AdminUser:Email"] = AdminEmail,
                    ["AdminUser:Password"] = AdminPassword,
                });
            });

            builder.ConfigureServices(services =>
            {
                // Removing only DbContextOptions<ApplicationDbContext> leaves the SQL Server
                // provider's own internal registrations (e.g. IDbContextOptionsConfiguration<T>)
                // behind, which then conflicts with InMemory ("only a single database provider can
                // be registered"). Remove everything AddDbContext<ApplicationDbContext> registered.
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                        || (d.ServiceType.IsGenericType && d.ServiceType.GetGenericArguments().Contains(typeof(ApplicationDbContext))))
                    .ToList();
                foreach (var d in descriptorsToRemove) services.Remove(d);

                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_dbName));
            });
        }
    }
}
