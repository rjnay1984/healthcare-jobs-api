using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.API.Extensions;

public static class UserTypeExtensions
{
    public static UserType? ParseUserType(string? userTypeString)
    {
        return string.IsNullOrEmpty(userTypeString)
            ? null
            : userTypeString.ToLowerInvariant() switch
            {
                "candidate" => UserType.Candidate,
                "employer" => UserType.Employer,
                "admin" => UserType.Admin,
                _ => null
            };
    }
}
