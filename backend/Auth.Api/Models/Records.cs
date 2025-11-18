using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Models;

// Requests
public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string FirstName,
    [Required] string LastName
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    bool RememberMe = false
);

public record RefreshTokenRequest(
    [Required] string Token,
    [Required] string RefreshToken
);

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    DateTime? DateOfBirth,
    string? LicenseNumber
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(6)] string NewPassword
);

public record AssignRoleRequest(
    [Required] string UserId,
    [Required] string RoleName
);

// Responses
public record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User
);

public record UserResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string UserType,
    string[] Roles,
    bool IsActive
);