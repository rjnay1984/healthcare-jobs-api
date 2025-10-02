using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Shared.DTOs;

public class EmployerProfileResponse
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public HealthcareOrganizationType OrganizationType { get; set; }
    public string? NPINumber { get; set; }
    public bool IsHIPPACompliant { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Website { get; set; }
}