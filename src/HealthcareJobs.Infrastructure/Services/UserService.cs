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
    public async Task CompleteOnboardingAsync(string authUserId, string email, UserSetupRequest request)
    {
        if (request.UserType == UserType.Candidate)
        {
            if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
                throw new ArgumentException("First name and last name are required for candidates");

            var candidate = new Candidate
            {
                Id = Guid.NewGuid(),
                AuthUserId = authUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ExperienceLevel = YearsOfExperience.EntryLevel,
                WillRelocate = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Candidates.Add(candidate);
        }
        else if (request.UserType == UserType.Employer)
        {
            if (string.IsNullOrEmpty(request.CompanyName))
                throw new ArgumentException("Company name is required for employers");

            var employer = new Employer
            {
                Id = Guid.NewGuid(),
                AuthUserId = authUserId,
                CompanyName = request.CompanyName,
                OrganizationType = request.OrganizationType ?? HealthcareOrganizationType.Other,
                IsHIPAACompliant = false,
                Description = string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            _context.Employers.Add(employer);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserType?> GetUserTypeAsync(string authUserId)
    {
        var isCandidate = await _context.Candidates
    .AnyAsync(c => c.AuthUserId == authUserId);

        if (isCandidate) return UserType.Candidate;

        var isEmployer = await _context.Employers
            .AnyAsync(e => e.AuthUserId == authUserId);

        return isEmployer ? UserType.Employer : null;
    }

    public async Task<bool> HasCompletedOnboardingAsync(string authUserId)
    {
        var hasCandidate = await _context.Candidates
            .AnyAsync(c => c.AuthUserId == authUserId);

        var hasEmployer = await _context.Employers
            .AnyAsync(e => e.AuthUserId == authUserId);

        return hasCandidate || hasEmployer;
    }
}