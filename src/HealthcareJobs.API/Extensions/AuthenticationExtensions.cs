using System.Security.Claims;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HealthcareJobs.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Only for development purposes
                options.SaveToken = true;

                var httpClient = new HttpClient();

                // Create a cached version
                var jwksCache = new Dictionary<string, JsonWebKey>();
                var jwksLastFetch = DateTime.MinValue;
                var jwksCacheDuration = TimeSpan.FromHours(1);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuers = ["http://localhost:3000"],
                    ValidAudiences = ["http://localhost:5000"],
                    ClockSkew = TimeSpan.Zero, // No clock skew
                    NameClaimType = "name",

                    IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                    {
                        // Check cache first
                        if (DateTime.UtcNow - jwksLastFetch < jwksCacheDuration && jwksCache.Count > 0)
                        {
                            return jwksCache.Values;
                        }

                        var httpClient = new HttpClient();
                        var jwksJson = httpClient.GetStringAsync("http://localhost:3000/api/auth/jwks").Result;

                        var keySet = new JsonWebKeySet(jwksJson);

                        // Update cache
                        jwksCache.Clear();
                        foreach (var key in keySet.Keys)
                        {
                            jwksCache[key.Kid] = key;
                        }
                        jwksLastFetch = DateTime.UtcNow;

                        return keySet.Keys;
                    }

                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication failed: " + context.Exception.Message);
                        Console.WriteLine($"Exception type: {context.Exception.GetType().Name}");
                        Console.WriteLine($"Stack trace: {context.Exception.StackTrace}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context => Task.CompletedTask,
                };
            });

        services.AddAuthorization();

        return services;
    }
}
