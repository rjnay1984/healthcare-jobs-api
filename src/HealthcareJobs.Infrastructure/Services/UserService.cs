using System.Security.Claims;

using HealthcareJobs.Core.Entities;
using HealthcareJobs.Core.Interfaces;
using HealthcareJobs.Infrastructure.Data;
using HealthcareJobs.Shared.DTOs;
using HealthcareJobs.Shared.Enums;

using Microsoft.EntityFrameworkCore;

namespace HealthcareJobs.Infrastructure.Services;

public class UserService(ApplicationDbContext context) : IUserService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<User?> GetUserByAuthentikSubjectAsync(string subject)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.AuthentikSubject == subject);
    }

    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal user)
    {
        var subject = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        return string.IsNullOrEmpty(subject) ? null : await GetUserByAuthentikSubjectAsync(subject);
    }

    public async Task<User> CreateUserAsync(string subject, string email, UserType userType)
    {
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            AuthentikSubject = subject,
            Email = email,
            Type = userType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }

    public async Task<bool> UserExistsAsync(string subject)
    {
        return await _context.Users.AnyAsync(u => u.AuthentikSubject == subject);
    }

    public async Task<User> CompleteUserSetupAsync(string subject, string email, UserSetupRequest request)
    {
        var user = await GetUserByAuthentikSubjectAsync(subject);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                AuthentikSubject = subject,
                Email = email,
                Type = request.UserType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        if (request.UserType == UserType.Candidate)
        {
            var candidateProfile = new Candidate
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                ExperienceLevel = YearsOfExperience.EntryLevel,
                WillRelocate = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Candidates.Add(candidateProfile);
        }
        else if (request.UserType == UserType.Employer)
        {
            if (string.IsNullOrEmpty(request.CompanyName))
            {
                throw new ArgumentException("CompanyName is required for Employer users.");
            }

            var employerProfile = new Employer
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CompanyName = request.CompanyName,
                OrganizationType = request.OrganizationType ?? HealthcareOrganizationType.Other,
                IsHIPAACompliant = false,
                Description = string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            _context.Employers.Add(employerProfile);
        }

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> IsUserSetupCompleteAsync(string subject)
    {
        var user = await GetUserByAuthentikSubjectAsync(subject);
        if (user == null) return false;

        if (user.Type == UserType.Candidate)
        {
            return await _context.Candidates.AnyAsync(c => c.UserId == user.Id);
        }
        else if (user.Type == UserType.Employer)
        {
            return await _context.Employers.AnyAsync(e => e.UserId == user.Id);
        }
        return false;
    }
}