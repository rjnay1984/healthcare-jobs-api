using HealthcareJobs.API.Extensions;
using HealthcareJobs.Core.Interfaces;
using HealthcareJobs.Shared.DTOs;

using System.Security.Claims;

namespace HealthcareJobs.API.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithOpenApi();

        group.MapPost("/setup", CompleteUserSetup)
            .RequireAuthorization()
            .WithName("CompleteUserSetup")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext context,
        IUserService userService)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? context.User.FindFirst("sub")?.Value;

        var email = context.User.FindFirst(ClaimTypes.Email)?.Value
                    ?? context.User.FindFirst("email")?.Value;

        var userTypeString = context.User.FindFirst("type")?.Value;
        var userType = UserTypeExtensions.ParseUserType(userTypeString);

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var hasCompletedOnboarding = await userService.HasCompletedOnboardingAsync(userId);

        return Results.Ok(new
        {
            userId,
            email,
            userType,
            name = context.User.Identity?.Name ?? null,
            hasCompletedOnboarding
        });
    }

    private static async Task<IResult> CompleteUserSetup(
        HttpContext context,
        UserSetupRequest request,
        IUserService userService)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? context.User.FindFirst("sub")?.Value;

        var email = context.User.FindFirst(ClaimTypes.Email)?.Value
                    ?? context.User.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            return Results.Unauthorized();

        try
        {
            await userService.CompleteOnboardingAsync(userId, email, request);

            return Results.Ok(new { message = "Onboarding completed successfully" });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}