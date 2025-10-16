using HealthcareJobs.API.Endpoints;
using HealthcareJobs.API.Extensions;
using HealthcareJobs.Core.Interfaces;
using HealthcareJobs.Infrastructure.Data;
using HealthcareJobs.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;


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

// Scoped Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJobService, JobService>();

// Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

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
app.UseCors("AllowFrontend");

var users = app.MapGroup("/api/users").WithTags("Users");
users.MapUserEndpoints();

var jobs = app.MapGroup("/api/jobs").WithTags("Jobs");
jobs.MapJobEndpoints();

app.Run();