using HealthcareJobs.Shared.DTOs;
using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Core.Interfaces;

public interface IUserService
{
    Task<UserType?> GetUserTypeAsync(string authUserId);
    Task<bool> HasCompletedOnboardingAsync(string authUserId);
    Task CompleteOnboardingAsync(string authUserId, string email, UserSetupRequest request);
}