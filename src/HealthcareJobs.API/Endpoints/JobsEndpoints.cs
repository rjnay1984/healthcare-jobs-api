using HealthcareJobs.Core.Interfaces;
using HealthcareJobs.Infrastructure.Data;
using HealthcareJobs.Shared.DTOs;
using HealthcareJobs.Shared.Enums;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareJobs.API.Endpoints;

public static class JobEndpoints
{
    public static RouteGroupBuilder MapJobEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", SearchJobs)
            .WithName("SearchJobs")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetJob)
            .WithName("GetJob")
            .WithOpenApi();

        group.MapPost("/", CreateJob)
            .RequireAuthorization()
            .WithName("CreateJob")
            .WithOpenApi();

        group.MapPut("/{id:guid}", UpdateJob)
            .RequireAuthorization()
            .WithName("UpdateJob")
            .WithOpenApi();

        group.MapDelete("/{id:guid}", DeleteJob)
            .RequireAuthorization()
            .WithName("DeleteJob")
            .WithOpenApi();

        group.MapPost("/{id:guid}/apply", ApplyToJob)
            .RequireAuthorization()
            .WithName("ApplyToJob")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> SearchJobs(
        IJobService jobService,
        string? keywords = null,
        bool? isRemote = null,
        string? city = null,
        string? state = null,
        YearsOfExperience? minExperience = null,
        decimal? minSalary = null,
        [FromQuery] int[]? certificationIds = null,
        [FromQuery] int[]? specialtyIds = null,
        HealthcareOrganizationType? organizationType = null,
        int page = 1,
        int pageSize = 20)
    {
        var request = new JobSearchRequest
        {
            Keywords = keywords,
            IsRemote = isRemote,
            City = city,
            State = state,
            MinExperience = minExperience,
            MinSalary = minSalary,
            CertificationIds = certificationIds?.ToList(),
            SpecialtyIds = specialtyIds?.ToList(),
            OrganizationType = organizationType,
            Page = page,
            PageSize = pageSize
        };

        var (jobList, totalCount) = await jobService.SearchJobsAsync(request);

        var response = jobList.Select(j => new JobResponse
        {
            Id = j.Id,
            Title = j.Title,
            Description = j.Description,
            MinExperience = j.MinExperience,
            SalaryMin = j.SalaryMin,
            SalaryMax = j.SalaryMax,
            IsRemote = j.IsRemote,
            RequiresLicense = j.RequiresLicense,
            PostedAt = j.PostedAt,
            ExpiresAt = j.ExpiresAt,
            Status = j.Status,
            EmployerId = j.EmployerId,
            CompanyName = j.Employer.CompanyName,
            OrganizationType = j.Employer.OrganizationType,
            Location = j.JobLocation != null ? new JobLocationResponse
            {
                City = j.JobLocation.City,
                State = j.JobLocation.State,
                ZipCode = j.JobLocation.ZipCode
            } : null,
            RequiredCertifications = j.RequiredCertifications.Select(c => c.Name).ToList(),
            RequiredSpecialties = j.RequiredSpecialties.Select(s => s.Name).ToList()
        }).ToList();

        return Results.Ok(new
        {
            jobs = response,
            totalCount = totalCount,
            page = request.Page,
            pageSize = request.PageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        });
    }

    private static async Task<IResult> GetJob(
        Guid id,
        IJobService jobService)
    {
        var job = await jobService.GetJobByIdAsync(id);
        if (job == null)
            return Results.NotFound();

        var response = new JobResponse
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            MinExperience = job.MinExperience,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            IsRemote = job.IsRemote,
            RequiresLicense = job.RequiresLicense,
            PostedAt = job.PostedAt,
            ExpiresAt = job.ExpiresAt,
            Status = job.Status,
            EmployerId = job.EmployerId,
            CompanyName = job.Employer.CompanyName,
            OrganizationType = job.Employer.OrganizationType,
            Location = job.JobLocation != null ? new JobLocationResponse
            {
                City = job.JobLocation.City,
                State = job.JobLocation.State,
                ZipCode = job.JobLocation.ZipCode
            } : null,
            RequiredCertifications = job.RequiredCertifications.Select(c => c.Name).ToList(),
            RequiredSpecialties = job.RequiredSpecialties.Select(s => s.Name).ToList()
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateJob(
        HttpContext context,
        CreateJobRequest request,
        IJobService jobService,
        IUserService userService,
        ApplicationDbContext dbContext)
    {
        var user = await userService.GetCurrentUserAsync(context.User);
        if (user == null)
            return Results.Unauthorized();

        if (user.Type != UserType.Employer)
            return Results.Forbid();

        var employer = await dbContext.Employers
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        if (employer == null)
            return Results.BadRequest(new { error = "Employer profile not found" });

        try
        {
            var job = await jobService.CreateJobAsync(employer.Id, request);
            return Results.Created($"/api/jobs/{job.Id}", new { id = job.Id });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateJob(
        Guid id,
        HttpContext context,
        UpdateJobRequest request,
        IJobService jobService,
        IUserService userService,
        ApplicationDbContext dbContext)
    {
        var user = await userService.GetCurrentUserAsync(context.User);
        if (user == null)
            return Results.Unauthorized();

        if (user.Type != UserType.Employer)
            return Results.Forbid();

        var employer = await dbContext.Employers
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        if (employer == null)
            return Results.BadRequest(new { error = "Employer profile not found" });

        try
        {
            var job = await jobService.UpdateJobAsync(id, employer.Id, request);
            return Results.Ok(new { id = job.Id, message = "Job updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteJob(
        Guid id,
        HttpContext context,
        IJobService jobService,
        IUserService userService,
        ApplicationDbContext dbContext)
    {
        var user = await userService.GetCurrentUserAsync(context.User);
        if (user == null)
            return Results.Unauthorized();

        if (user.Type != UserType.Employer)
            return Results.Forbid();

        var employer = await dbContext.Employers
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        if (employer == null)
            return Results.BadRequest(new { error = "Employer profile not found" });

        var deleted = await jobService.DeleteJobAsync(id, employer.Id);

        if (!deleted)
            return Results.NotFound();

        return Results.NoContent();
    }

    private static async Task<IResult> ApplyToJob(
        Guid id,
        HttpContext context,
        JobApplicationRequest request,
        IJobService jobService,
        IUserService userService,
        ApplicationDbContext dbContext)
    {
        var user = await userService.GetCurrentUserAsync(context.User);
        if (user == null)
            return Results.Unauthorized();

        if (user.Type != UserType.Candidate)
            return Results.Forbid();

        var candidate = await dbContext.Candidates
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (candidate == null)
            return Results.BadRequest(new { error = "Candidate profile not found" });

        try
        {
            var application = await jobService.ApplyToJobAsync(id, candidate.Id, request);
            return Results.Ok(new
            {
                applicationId = application.Id,
                message = "Application submitted successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }
}