using System.Security.Claims;

using HealthcareJobs.Core.Interfaces;
using HealthcareJobs.Shared.DTOs;

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
        var authentikSubject = GetClaimValue(context.User,
            ClaimTypes.NameIdentifier,
            "sub");

        var email = GetClaimValue(context.User,
            ClaimTypes.Email,
            "email");

        if (string.IsNullOrEmpty(authentikSubject))
            return Results.Unauthorized();

        var user = await userService.GetUserByAuthentikSubjectAsync(authentikSubject);
        var isSetupComplete = await userService.IsUserSetupCompleteAsync(authentikSubject);

        if (user == null)
        {
            return Results.Ok(new
            {
                exists = false,
                isSetupComplete = false,
                email = email
            });
        }

        return Results.Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Type = user.Type,
            IsActive = user.IsActive,
            IsSetupComplete = isSetupComplete,
            CreatedAt = user.CreatedAt
        });
    }

    private static async Task<IResult> CompleteUserSetup(
        HttpContext context,
        UserSetupRequest request,
        IUserService userService)
    {
        var authentikSubject = GetClaimValue(context.User,
            ClaimTypes.NameIdentifier,
            "sub");

        var email = GetClaimValue(context.User,
            ClaimTypes.Email,
            "email");

        if (string.IsNullOrEmpty(authentikSubject) || string.IsNullOrEmpty(email))
            return Results.Unauthorized();

        try
        {
            var user = await userService.CompleteUserSetupAsync(authentikSubject, email, request);
            var isSetupComplete = await userService.IsUserSetupCompleteAsync(authentikSubject);

            return Results.Ok(new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Type = user.Type,
                IsActive = user.IsActive,
                IsSetupComplete = isSetupComplete,
                CreatedAt = user.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static string? GetClaimValue(System.Security.Claims.ClaimsPrincipal? user, params string[] claimTypes)
    {
        if (user == null) return null;
        foreach (var claimType in claimTypes)
        {
            var claim = user.FindFirst(claimType);
            if (claim != null) return claim.Value;
        }
        return null;
    }
}