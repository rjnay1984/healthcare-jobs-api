using System.Security.Claims;

using HealthcareJobs.Infrastructure.Data;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// DEBUG: Log configuration values
Console.WriteLine($"Authority: {builder.Configuration["Authentik:Authority"]}");
Console.WriteLine($"ClientId: {builder.Configuration["Authentik:ClientId"]}");
Console.WriteLine($"MetadataAddress: {builder.Configuration["Authentik:MetadataAddress"]}");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        options.MetadataAddress = builder.Configuration["Authentik:MetadataAddress"];
#pragma warning restore CS8601 // Possible null reference assignment.
        options.RequireHttpsMetadata = false; // Only for development purposes

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false,
            ValidIssuers = [builder.Configuration["Authentik:Authority"]],
            ValidAudiences = [builder.Configuration["Authentik:ClientId"]],
            ClockSkew = TimeSpan.FromMinutes(5), // Allow a 5 minute clock skew
            NameClaimType = ClaimTypes.GivenName,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
                Console.WriteLine($"Received token (first 50 chars): {token?.Substring(0, Math.Min(50, token?.Length ?? 0))}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed: " + context.Exception.Message);
                Console.WriteLine($"Exception type: {context.Exception.GetType().Name}");
                Console.WriteLine($"Stack trace: {context.Exception.StackTrace}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated for: " + (context.Principal?.Identity?.Name ?? "unknown"));
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
    policy.WithOrigins((builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:3000").Split(",")) // Adjust the origin as needed
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));






var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/api/test/protected", (HttpContext context) =>
{
    var ctxUser = context.User;
    if (ctxUser == null || ctxUser.Identity == null || !ctxUser.Identity.IsAuthenticated)
    {
        return Results.Unauthorized();
    }
    var claims = ctxUser.Claims;
    var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "unknown";
    var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "unknown";

    return Results.Ok(new
    {
        Message = "You have accessed a protected endpoint!",
        UserId = userId,
        Email = email,
        Claims = claims.Select(c => new { c.Type, c.Value })
    });
})
.WithName("GetProtected")
.WithOpenApi()
.RequireAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
