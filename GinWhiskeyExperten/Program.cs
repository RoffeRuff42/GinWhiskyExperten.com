using GinWhiskeyExperten.Data;
using GinWhiskeyExperten.Repositories;
using GinWhiskeyExperten.Services;
using GinWhiskeyExperten.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Dependency Injection (Lager/Layers)
// This tells the app which classes to use for the interfaces
builder.Services.AddScoped<ISpiritRepository, SpiritRepository>();
builder.Services.AddScoped<ISpiritService, SpiritService>();

builder.Services.AddHttpClient<SystembolagetIntegrationService>(); // Register the HTTP client for the Systembolaget integration service
builder.Services.AddHttpClient<CocktailIntegrationService>(); // Register the HTTP client for the cocktail integration service

builder.Services.ConfigureCors(); // Add CORS configuration from the extension method
builder.Services.ConfigureRateLimiting(); // Register the rate limiter
builder.Services.AddMemoryCache();
builder.Services.AddControllers();

// 3. Swagger/OpenAPI Configuration
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Configure the HTTP request pipeline (Middleware)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();  
    app.UseSwaggerUI(); 
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy"); // Use the defined CORS policy
app.UseRateLimiter(); // Enable the middleware

app.UseAuthorization();
app.MapControllers();

app.Run();