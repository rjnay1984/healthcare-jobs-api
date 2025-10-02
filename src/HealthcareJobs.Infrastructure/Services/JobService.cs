// src/HealthcareJobs.Infrastructure/Services/JobService.cs
using HealthcareJobs.Core.Entities;
using HealthcareJobs.Core.Interfaces;
using HealthcareJobs.Infrastructure.Data;
using HealthcareJobs.Shared.DTOs;
using HealthcareJobs.Shared.Enums;

using Microsoft.EntityFrameworkCore;

namespace HealthcareJobs.Infrastructure.Services;

public class JobService : IJobService
{
    private readonly ApplicationDbContext _context;

    public JobService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobPosting> CreateJobAsync(Guid employerId, CreateJobRequest request)
    {
        var employer = await _context.Employers.FindAsync(employerId);
        if (employer == null)
            throw new ArgumentException("Employer not found");

        Address? location = null;
        if (!request.IsRemote && !string.IsNullOrEmpty(request.LocationCity))
        {
            location = new Address
            {
                Id = Guid.NewGuid(),
                Street = request.LocationStreet ?? string.Empty,
                City = request.LocationCity,
                State = request.LocationState ?? string.Empty,
                ZipCode = request.LocationZipCode ?? string.Empty,
                Country = "United States",
                CreatedAt = DateTime.UtcNow
            };
            _context.Addresses.Add(location);
        }

        var job = new JobPosting
        {
            Id = Guid.NewGuid(),
            EmployerId = employerId,
            Title = request.Title,
            Description = request.Description,
            MinExperience = request.MinExperience,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            IsRemote = request.IsRemote,
            RequiresLicense = request.RequiresLicense,
            PostedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresInDays.HasValue
                ? DateTime.UtcNow.AddDays(request.ExpiresInDays.Value)
                : null,
            Status = JobStatus.Active,
            JobLocation = location,
            CreatedAt = DateTime.UtcNow
        };

        // Add certifications
        if (request.RequiredCertificationIds.Any())
        {
            var certifications = await _context.Certifications
                .Where(c => request.RequiredCertificationIds.Contains(c.Id))
                .ToListAsync();
            job.RequiredCertifications = certifications;
        }

        // Add specialties
        if (request.RequiredSpecialtyIds.Any())
        {
            var specialties = await _context.Specialties
                .Where(s => request.RequiredSpecialtyIds.Contains(s.Id))
                .ToListAsync();
            job.RequiredSpecialties = specialties;
        }

        _context.JobPostings.Add(job);
        await _context.SaveChangesAsync();

        return job;
    }

    public async Task<JobPosting?> GetJobByIdAsync(Guid jobId)
    {
        return await _context.JobPostings
            .Include(j => j.Employer)
            .Include(j => j.JobLocation)
            .Include(j => j.RequiredCertifications)
            .Include(j => j.RequiredSpecialties)
            .FirstOrDefaultAsync(j => j.Id == jobId);
    }

    public async Task<(List<JobPosting> jobs, int totalCount)> SearchJobsAsync(JobSearchRequest request)
    {
        var query = _context.JobPostings
            .Include(j => j.Employer)
            .Include(j => j.JobLocation)
            .Include(j => j.RequiredCertifications)
            .Include(j => j.RequiredSpecialties)
            .Where(j => j.Status == JobStatus.Active)
            .AsQueryable();

        // Keyword search (title or description)
        if (!string.IsNullOrEmpty(request.Keywords))
        {
            var keywords = request.Keywords.ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(keywords) ||
                j.Description.ToLower().Contains(keywords));
        }

        // Remote filter
        if (request.IsRemote.HasValue)
        {
            query = query.Where(j => j.IsRemote == request.IsRemote.Value);
        }

        // Location filters
        if (!string.IsNullOrEmpty(request.City))
        {
            query = query.Where(j => j.JobLocation != null &&
                j.JobLocation.City.ToLower() == request.City.ToLower());
        }

        if (!string.IsNullOrEmpty(request.State))
        {
            query = query.Where(j => j.JobLocation != null &&
                j.JobLocation.State.ToLower() == request.State.ToLower());
        }

        // Experience filter
        if (request.MinExperience.HasValue)
        {
            query = query.Where(j => j.MinExperience <= request.MinExperience.Value);
        }

        // Salary filter
        if (request.MinSalary.HasValue)
        {
            query = query.Where(j => j.SalaryMin >= request.MinSalary.Value ||
                j.SalaryMax >= request.MinSalary.Value);
        }

        // Certification filter
        if (request.CertificationIds != null && request.CertificationIds.Any())
        {
            query = query.Where(j => j.RequiredCertifications
                .Any(c => request.CertificationIds.Contains(c.Id)));
        }

        // Specialty filter
        if (request.SpecialtyIds != null && request.SpecialtyIds.Any())
        {
            query = query.Where(j => j.RequiredSpecialties
                .Any(s => request.SpecialtyIds.Contains(s.Id)));
        }

        // Organization type filter
        if (request.OrganizationType.HasValue)
        {
            query = query.Where(j => j.Employer.OrganizationType == request.OrganizationType.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var jobs = await query
            .OrderByDescending(j => j.PostedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (jobs, totalCount);
    }

    public async Task<JobPosting> UpdateJobAsync(Guid jobId, Guid employerId, UpdateJobRequest request)
    {
        var job = await _context.JobPostings
            .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerId == employerId);

        if (job == null)
            throw new ArgumentException("Job not found or you don't have permission to update it");

        if (!string.IsNullOrEmpty(request.Title))
            job.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            job.Description = request.Description;

        if (request.MinExperience.HasValue)
            job.MinExperience = request.MinExperience.Value;

        if (request.SalaryMin.HasValue)
            job.SalaryMin = request.SalaryMin;

        if (request.SalaryMax.HasValue)
            job.SalaryMax = request.SalaryMax;

        if (request.IsRemote.HasValue)
            job.IsRemote = request.IsRemote.Value;

        if (request.Status.HasValue)
            job.Status = request.Status.Value;

        job.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<bool> DeleteJobAsync(Guid jobId, Guid employerId)
    {
        var job = await _context.JobPostings
            .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerId == employerId);

        if (job == null)
            return false;

        _context.JobPostings.Remove(job);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<JobApplication> ApplyToJobAsync(Guid jobId, Guid candidateId, JobApplicationRequest request)
    {
        var job = await _context.JobPostings.FindAsync(jobId);
        if (job == null || job.Status != JobStatus.Active)
            throw new ArgumentException("Job not found or is no longer active");

        var candidate = await _context.Candidates.FindAsync(candidateId);
        if (candidate == null)
            throw new ArgumentException("Candidate not found");

        // Check if already applied
        var existingApplication = await _context.JobApplications
            .FirstOrDefaultAsync(a => a.JobPostingId == jobId && a.CandidateId == candidateId);

        if (existingApplication != null)
            throw new InvalidOperationException("You have already applied to this job");

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            JobPostingId = jobId,
            AppliedAt = DateTime.UtcNow,
            Status = ApplicationStatus.Submitted,
            CoverLetter = request.CoverLetter,
            CreatedAt = DateTime.UtcNow
        };

        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();

        return application;
    }

    public async Task<bool> HasAlreadyAppliedAsync(Guid jobId, Guid candidateId)
    {
        return await _context.JobApplications
            .AnyAsync(a => a.JobPostingId == jobId && a.CandidateId == candidateId);
    }
}