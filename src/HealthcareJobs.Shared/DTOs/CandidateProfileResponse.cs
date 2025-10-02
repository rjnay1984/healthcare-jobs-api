using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Shared.DTOs;

public class CandidateProfileResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public LicenseType? LicenseType { get; set; }
    public YearsOfExperience ExperienceLevel { get; set; }
    public decimal? DesiredSalaryMin { get; set; }
    public bool WillRelocate { get; set; }
}