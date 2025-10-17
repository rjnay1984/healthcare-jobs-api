using HealthcareJobs.API.Extensions;
using HealthcareJobs.Core.Interfaces;
using HealthcareJobs.Infrastructure.Data;
using HealthcareJobs.Shared.DTOs;
using HealthcareJobs.Shared.Enums;

using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

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
        HttpContext context,
        IJobService jobService)
    {
        // Manually parse query parameters
        var request = new JobSearchRequest
        {
            Keywords = context.Request.Query["keywords"].FirstOrDefault(),
            IsRemote = bool.TryParse(context.Request.Query["isRemote"].FirstOrDefault(), out var isRemote) ? isRemote : null,
            City = context.Request.Query["city"].FirstOrDefault(),
            State = context.Request.Query["state"].FirstOrDefault(),
            MinExperience = Enum.TryParse<YearsOfExperience>(context.Request.Query["minExperience"].FirstOrDefault(), out var exp) ? exp : null,
            MinSalary = decimal.TryParse(context.Request.Query["minSalary"].FirstOrDefault(), out var salary) ? salary : null,
            CertificationIds = [.. context.Request.Query["certificationIds"]
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)],
            SpecialtyIds = [.. context.Request.Query["specialtyIds"]
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)],
            OrganizationType = Enum.TryParse<HealthcareOrganizationType>(context.Request.Query["organizationType"].FirstOrDefault(), out var orgType) ? orgType : null,
            Page = int.TryParse(context.Request.Query["page"].FirstOrDefault(), out var page) ? page : 1,
            PageSize = int.TryParse(context.Request.Query["pageSize"].FirstOrDefault(), out var pageSize) ? pageSize : 20
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
            RequiredCertifications = [.. j.RequiredCertifications.Select(c => c.Name)],
            RequiredSpecialties = [.. j.RequiredSpecialties.Select(s => s.Name)]
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
            RequiredCertifications = [.. job.RequiredCertifications.Select(c => c.Name)],
            RequiredSpecialties = [.. job.RequiredSpecialties.Select(s => s.Name)]
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
        var userId = GetUserId(context);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        // Check if user is an employer
        var userTypeString = context.User.FindFirst("type")?.Value;
        var userType = UserTypeExtensions.ParseUserType(userTypeString);
        if (userType != UserType.Employer)
            return Results.Forbid();

        // Get employer entity
        var employer = await dbContext.Employers
            .FirstOrDefaultAsync(e => e.AuthUserId == userId);

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
        var userId = GetUserId(context);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        // Check if user is an employer
        var userType = await userService.GetUserTypeAsync(userId);
        if (userType != UserType.Employer)
            return Results.Forbid();

        // Get employer entity
        var employer = await dbContext.Employers
            .FirstOrDefaultAsync(e => e.AuthUserId == userId);

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
        var userId = GetUserId(context);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        // Check if user is an employer
        var userType = await userService.GetUserTypeAsync(userId);
        if (userType != UserType.Employer)
            return Results.Forbid();

        // Get employer entity
        var employer = await dbContext.Employers
            .FirstOrDefaultAsync(e => e.AuthUserId == userId);

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
        var userId = GetUserId(context);
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        // Check if user is a candidate
        var userType = await userService.GetUserTypeAsync(userId);
        if (userType != UserType.Candidate)
            return Results.Forbid();

        // Get candidate entity
        var candidate = await dbContext.Candidates
            .FirstOrDefaultAsync(c => c.AuthUserId == userId);

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

    // Helper method to get user ID from claims
    private static string? GetUserId(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? context.User.FindFirst("sub")?.Value
               ?? context.User.FindFirst("userId")?.Value;
    }
}