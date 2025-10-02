// src/HealthcareJobs.Shared/DTOs/JobSearchRequest.cs
using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Shared.DTOs;

public class JobSearchRequest
{
    public string? Keywords { get; set; }
    public bool? IsRemote { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public YearsOfExperience? MinExperience { get; set; }
    public decimal? MinSalary { get; set; }
    public List<int>? CertificationIds { get; set; }
    public List<int>? SpecialtyIds { get; set; }
    public HealthcareOrganizationType? OrganizationType { get; set; }

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}