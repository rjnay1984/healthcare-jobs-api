using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Shared.DTOs;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserType Type { get; set; }
    public bool IsActive { get; set; }
    public bool IsSetupComplete { get; set; }
    public DateTime CreatedAt { get; set; }
}