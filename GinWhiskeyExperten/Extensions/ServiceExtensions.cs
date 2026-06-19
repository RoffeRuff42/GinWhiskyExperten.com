using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace GinWhiskeyExperten.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.WithOrigins("http://localhost:3000", "https://localhost:5173") //Specify the allowed client origins e.g future frontend
                           .AllowAnyMethod()
                           .AllowAnyHeader());

            });
        }

        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("FixedWindowPolicy", opt =>
                {
                    opt.PermitLimit = 10; // Max 10 requests..
                    opt.Window = TimeSpan.FromMinutes(1); //.. per minute
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // If the limit is exceeded, queue
                    opt.QueueLimit = 2; // Allow up to 2 queued requests

                });

                // Stricter policy for the login endpoint - a credential-stuffing/brute-force target,
                // not covered well by the general-purpose 10/min policy above.
                options.AddFixedWindowLimiter("AuthPolicy", opt =>
                {
                    opt.PermitLimit = 5; // Max 5 login attempts..
                    opt.Window = TimeSpan.FromMinutes(1); //.. per minute
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0; // No queueing - reject immediately past the limit
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; // Return 429 when rate limit is exceeded
            });
        }
    }
}
