// src/HealthcareJobs.Shared/DTOs/CreateJobRequest.cs
using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Shared.DTOs;

public class CreateJobRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public YearsOfExperience MinExperience { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public bool IsRemote { get; set; }
    public bool RequiresLicense { get; set; }
    public int? ExpiresInDays { get; set; } = 30;

    // Location (optional if remote)
    public string? LocationStreet { get; set; }
    public string? LocationCity { get; set; }
    public string? LocationState { get; set; }
    public string? LocationZipCode { get; set; }

    // Requirements
    public List<int> RequiredCertificationIds { get; set; } = new();
    public List<int> RequiredSpecialtyIds { get; set; } = new();
}