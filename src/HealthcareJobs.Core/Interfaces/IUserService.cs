using System.Security.Claims;

using HealthcareJobs.Core.Entities;
using HealthcareJobs.Shared.DTOs;
using HealthcareJobs.Shared.Enums;

namespace HealthcareJobs.Core.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByAuthentikSubjectAsync(string subject);
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal user);
    Task<User> CreateUserAsync(string subject, string email, UserType userType);
    Task<bool> UserExistsAsync(string subject);
    Task<User> CompleteUserSetupAsync(string subject, string email, UserSetupRequest request);
    Task<bool> IsUserSetupCompleteAsync(string subject);
}