using Auth.Api.Models;

namespace Auth.Api.Services;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task RevokeTokenAsync(string userId);
    Task<UserResponse> GetUserByIdAsync(string userId);
    Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task AssignRoleAsync(AssignRoleRequest request);
}