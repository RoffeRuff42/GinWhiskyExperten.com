using GinWhiskeyExperten.Data;
using GinWhiskeyExperten.Repositories;
using GinWhiskeyExperten.Services;
using GinWhiskeyExperten.Extensions;
using GinWhiskeyExperten.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Dependency Injection (Lager/Layers)
// This tells the app which classes to use for the interfaces
builder.Services.AddScoped<ISpiritRepository, SpiritRepository>();
builder.Services.AddScoped<ISpiritService, SpiritService>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IFlavorRepository, FlavorRepository>();
builder.Services.AddScoped<IFlavorService, FlavorService>();

builder.Services.AddHttpClient<SystembolagetIntegrationService>(); // Register the HTTP client for the Systembolaget integration service
builder.Services.AddHttpClient<CocktailIntegrationService>(); // Register the HTTP client for the cocktail integration service

builder.Services.ConfigureCors(); // Add CORS configuration from the extension method
builder.Services.ConfigureRateLimiting(); // Register the rate limiter
builder.Services.AddMemoryCache();
builder.Services.AddControllers();

// 2b. Identity + JWT authentication
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8; // keep Identity's other defaults (upper/lower/digit/non-alphanumeric)
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Fail fast at startup if misconfigured - but deliberately NOT captured into a local variable for
// reuse below: AddJwtBearer's options delegate runs lazily (on first auth attempt, well after
// startup), so the signing key must be re-read from configuration at that point too, not from a
// value snapshotted here. Capturing it eagerly caused AuthController (which reads IConfiguration
// live per-request) and the validator (which would have used this stale snapshot) to sign/validate
// with different keys whenever configuration changed after this line ran (e.g. under
// WebApplicationFactory in tests, where test config is layered in around this exact point).
_ = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured. Set it via User Secrets locally or App Service settings in Azure.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
    });

builder.Services.AddAuthorization();

// 3. Swagger/OpenAPI Configuration
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT token returned by POST /api/auth/login"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 4. Seed the Admin role/user (idempotent - safe on every startup)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");
    await IdentitySeeder.SeedAsync(scope.ServiceProvider, app.Configuration, logger);
}

// 5. Configure the HTTP request pipeline (Middleware)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy"); // Use the defined CORS policy
app.UseRateLimiter(); // Enable the middleware

app.UseAuthentication(); // Must precede UseAuthorization - establishes the caller's identity from the JWT
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { } // Makes the implicit Program class visible to WebApplicationFactory<Program> in tests
