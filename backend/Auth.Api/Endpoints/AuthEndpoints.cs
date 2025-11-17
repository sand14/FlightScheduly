using System.Security.Claims;
using Auth.Api.Constants;
using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // Public endpoints
        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh-token", RefreshTokenAsync)
            .WithName("RefreshToken")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        // Protected endpoints
        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/me", UpdateCurrentUserAsync)
            .WithName("UpdateCurrentUser")
            .RequireAuthorization()
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Admin-only endpoints
        group.MapPost("/assign-role", AssignRoleAsync)
            .WithName("AssignRole")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrator))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/user/{userId}", GetUserByIdAsync)
            .WithName("GetUserById")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrator))
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IAuthService authService,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Registration attempt for email: {Email}", request.Email);
            var user = await authService.RegisterAsync(request);
            return Results.Created($"/api/auth/user/{user.Id}", user);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails 
            { 
                Status = 400, 
                Title = "Invalid request", 
                Detail = ex.Message 
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ProblemDetails 
            { 
                Status = 400, 
                Title = "Registration failed", 
                Detail = ex.Message 
            });
        }
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IAuthService authService,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Login attempt for email: {Email}", request.Email);
            var response = await authService.LoginAsync(request);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: 401,
                title: "Authentication failed",
                detail: ex.Message
            );
        }
    }

    private static async Task<IResult> RefreshTokenAsync(
        RefreshTokenRequest request,
        IAuthService authService)
    {
        try
        {
            var response = await authService.RefreshTokenAsync(request);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: 401,
                title: "Token refresh failed",
                detail: ex.Message
            );
        }
    }

    private static async Task<IResult> LogoutAsync(
        ClaimsPrincipal user,
        IAuthService authService)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            await authService.RevokeTokenAsync(userId);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                statusCode: 404,
                title: "User not found",
                detail: ex.Message
            );
        }
    }

    private static async Task<IResult> GetCurrentUserAsync(
        ClaimsPrincipal user,
        IAuthService authService)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var userResponse = await authService.GetUserByIdAsync(userId);
            return Results.Ok(userResponse);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                statusCode: 404,
                title: "User not found",
                detail: ex.Message
            );
        }
    }

    private static async Task<IResult> UpdateCurrentUserAsync(
        UpdateUserRequest request,
        ClaimsPrincipal user,
        IAuthService authService)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var userResponse = await authService.UpdateUserAsync(userId, request);
            return Results.Ok(userResponse);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                statusCode: 404,
                title: "User not found",
                detail: ex.Message
            );
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ProblemDetails 
            { 
                Status = 400, 
                Title = "Update failed", 
                Detail = ex.Message 
            });
        }
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        IAuthService authService)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            await authService.ChangePasswordAsync(userId, request);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                statusCode: 404,
                title: "User not found",
                detail: ex.Message
            );
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ProblemDetails 
            { 
                Status = 400, 
                Title = "Password change failed", 
                Detail = ex.Message 
            });
        }
    }

    private static async Task<IResult> AssignRoleAsync(
        AssignRoleRequest request,
        IAuthService authService)
    {
        try
        {
            await authService.AssignRoleAsync(request);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                statusCode: 404,
                title: "Not found",
                detail: ex.Message
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails 
            { 
                Status = 400, 
                Title = "Invalid role", 
                Detail = ex.Message 
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ProblemDetails 
            { 
                Status = 400, 
                Title = "Role assignment failed", 
                Detail = ex.Message 
            });
        }
    }

    private static async Task<IResult> GetUserByIdAsync(
        string userId,
        IAuthService authService)
    {
        try
        {
            var userResponse = await authService.GetUserByIdAsync(userId);
            return Results.Ok(userResponse);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(
                statusCode: 404,
                title: "User not found",
                detail: ex.Message
            );
        }
    }
}
