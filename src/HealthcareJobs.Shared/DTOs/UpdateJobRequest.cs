// src/HealthcareJobs.Shared/DTOs/UpdateJobRequest.cs
using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Shared.DTOs;

public class UpdateJobRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public YearsOfExperience? MinExperience { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public bool? IsRemote { get; set; }
    public JobStatus? Status { get; set; }
}