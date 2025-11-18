using Auth.Api.Models;

namespace Auth.Api.Services;

public interface IAuthService
{
    /// <summary>
/// Registers a new user using the provided registration data.
/// </summary>
/// <param name="request">Registration details such as credentials and profile information.</param>
/// <returns>A UserResponse containing the created user's information.</returns>
Task<UserResponse> RegisterAsync(RegisterRequest request);
    /// <summary>
/// Authenticates a user using the provided credentials and returns authentication details.
/// </summary>
/// <param name="request">Credentials and options required to authenticate the user.</param>
/// <returns>A LoginResponse containing access and refresh tokens and user information.</returns>
Task<LoginResponse> LoginAsync(LoginRequest request);
    /// <summary>
/// Refreshes authentication tokens using the supplied refresh token data.
/// </summary>
/// <param name="request">The refresh token request containing the refresh token and any required metadata.</param>
/// <returns>A LoginResponse containing new authentication tokens and related login details.</returns>
Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
    /// <summary>
/// Revokes all active authentication and refresh tokens associated with the specified user.
/// </summary>
/// <param name="userId">The unique identifier of the user whose tokens will be revoked.</param>
Task RevokeTokenAsync(string userId);
    /// <summary>
/// Retrieves information for the user with the specified identifier.
/// </summary>
/// <param name="userId">The unique identifier of the user to retrieve.</param>
/// <returns>The user's details.</returns>
Task<UserResponse> GetUserByIdAsync(string userId);
    /// <summary>
/// Updates the specified user's profile with the provided update data.
/// </summary>
/// <param name="userId">Identifier of the user to update.</param>
/// <param name="request">Data containing the fields to update for the user.</param>
/// <returns>The updated user information.</returns>
Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);
    /// <summary>
/// Changes the password for the specified user using the provided current and new password data.
/// </summary>
/// <param name="userId">Identifier of the user whose password will be changed.</param>
/// <param name="request">Contains the current password and the new password to apply.</param>
Task ChangePasswordAsync(string userId, ChangePasswordRequest request);
    /// <summary>
/// Assigns one roles to a user according to the provided role assignment data.
/// </summary>
/// <param name="request">Role assignment details including the target user ID and the role ID to assign.</param>
Task AssignRoleAsync(AssignRoleRequest request);
}