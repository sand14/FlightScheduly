using Auth.Api.Constants;
using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using System.Security.Claims;

namespace Auth.Api.Endpoints;

public static class AuthEndpoints
{
    /// <summary>
    /// Registers authentication-related HTTP endpoints under /api/auth on the provided route builder.
    /// </summary>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance with authentication endpoints mapped.</returns>
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
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound); ;

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
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // Admin-only endpoints
        group.MapPost("/assign-role", AssignRoleAsync)
            .WithName("AssignRole")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrator))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/user/{userId}", GetUserByIdAsync)
            .WithName("GetUserById")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrator))
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Handles a user registration request and returns the created user or a validation/error response.
    /// </summary>
    /// <param name="request">Registration details such as email and password.</param>
    /// <param name="authService">Authentication service used to create the user.</param>
    /// <param name="logger">Logger for recording the registration attempt.</param>
    /// <returns>`IResult` producing 201 Created with the created user on success, or 400 Bad Request with a `ProblemDetails` describing the error.</returns>
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

    /// <summary>
    /// Authenticate a user using the provided credentials and return an HTTP result representing success or failure.
    /// </summary>
    /// <param name="request">The login credentials (for example, email and password).</param>
    /// <returns>`200 OK` with the authentication response on success; `401 Unauthorized` with `ProblemDetails` when authentication fails.</returns>
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

    /// <summary>
    /// Refreshes an authentication token and returns the corresponding HTTP result.
    /// </summary>
    /// <param name="request">The refresh token request containing the token to be refreshed.</param>
    /// <param name="authService">Authentication service used to perform the refresh operation.</param>
    /// <returns>`200 OK` with the refreshed token response on success, or a `401` ProblemDetails when the token cannot be refreshed.</returns>
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

    /// <summary>
    /// Revokes the authenticated user's refresh token to log them out.
    /// </summary>
    /// <param name="user">The authenticated principal whose identifier is used to determine which user's tokens to revoke.</param>
    /// <param name="authService">Authentication Service that does the work.</param>
    /// <returns>`204 NoContent` on success; a `404` ProblemDetails response if the user is not found.</returns>
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

    /// <summary>
    /// Gets the details of the currently authenticated user.
    /// </summary>
    /// <param name="user">The authenticated principal; the user's identifier is read from the `NameIdentifier` claim.</param>
    /// <returns>`200 OK` with the user's details on success, or a `404` ProblemDetails response when the user is not found.</returns>
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

    /// <summary>
    /// Updates the authenticated user's profile with the provided values and returns the updated user.
    /// </summary>
    /// <param name="request">Fields to update on the current user's profile.</param>
    /// <param name="user">The authenticated principal; the user's identifier is read from the NameIdentifier claim.</param>
    /// <param name="authService">Service used to perform the user update operation.</param>
    /// <returns>
    /// An HTTP result:
    /// - 200 OK with the updated user on success;
    /// - 404 ProblemDetails if the user was not found;
    /// - 400 ProblemDetails if the update failed due to invalid operation.
    /// </returns>
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

    /// <summary>
    /// Changes the password for the currently authenticated user.
    /// </summary>
    /// <param name="request">The password change details (current and new password).</param>
    /// <param name="user">The authenticated principal whose NameIdentifier claim identifies the target user.</param>
    /// <returns>
    /// An HTTP result:
    /// - 204 NoContent on success.
    /// - 404 with ProblemDetails (title: "User not found") if the user does not exist.
    /// - 400 BadRequest with ProblemDetails (title: "Password change failed") if the password change is invalid.
    /// </returns>
    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        IAuthService authService)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Problem(
                                 statusCode: StatusCodes.Status401Unauthorized,
                                 title: "Unauthorized",
                                 detail: "User identifier is missing from the authentication token.");
            }

            await authService.RevokeTokenAsync(userId);

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

    /// <summary>
    /// Assigns a role to a user based on the provided request.
    /// </summary>
    /// <param name="request">Request containing the target user identifier and the role to assign.</param>
    /// <returns>`204 NoContent` on success; a `404` ProblemDetails if the target user is not found; `400` ProblemDetails if the role is invalid or the assignment fails.</returns>
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

    /// <summary>
    /// Retrieves a user by their identifier and returns an HTTP result representing the outcome.
    /// </summary>
    /// <param name="userId">The identifier of the user to retrieve.</param>
    /// <returns>An <see cref="IResult"/> that is 200 OK with the user's data when found, or 404 with ProblemDetails when the user does not exist.</returns>
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