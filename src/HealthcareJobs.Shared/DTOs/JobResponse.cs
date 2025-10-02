// src/HealthcareJobs.Shared/DTOs/JobResponse.cs
using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Shared.DTOs;

public class JobResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public YearsOfExperience MinExperience { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public bool IsRemote { get; set; }
    public bool RequiresLicense { get; set; }
    public DateTime PostedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public JobStatus Status { get; set; }

    // Employer info
    public Guid EmployerId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public HealthcareOrganizationType OrganizationType { get; set; }

    // Location
    public JobLocationResponse? Location { get; set; }

    // Requirements
    public List<string> RequiredCertifications { get; set; } = new();
    public List<string> RequiredSpecialties { get; set; } = new();
}

public class JobLocationResponse
{
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}