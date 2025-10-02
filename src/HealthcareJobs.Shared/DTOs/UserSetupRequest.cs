using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Shared.DTOs;

public class UserSetupRequest
{
    public UserType UserType { get; set; }


    // For Candidates
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // For Employers
    public string? CompanyName { get; set; }
    public HealthcareOrganizationType? OrganizationType { get; set; }
}