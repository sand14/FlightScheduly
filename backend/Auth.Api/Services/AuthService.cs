using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.Api.Configuration;
using Auth.Api.Constants;
using Auth.Api.Data;
using Auth.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Initializes a new instance of AuthService with the required ASP.NET Identity managers and JWT settings.
    /// </summary>
    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Creates a new user account from the provided registration details and assigns the "Student" role if that role exists.
    /// </summary>
    /// <param name="request">Registration data containing email, password, first name, and last name.</param>
    /// <returns>A <see cref="UserResponse"/> representing the newly created user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when user creation fails; the exception message contains concatenated identity error descriptions.</exception>
    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var roleName = Roles.Student;
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        return await MapToUserResponse(user);
    }

    /// <summary>
    /// Authenticates a user with email and password and returns JWT and refresh token information.
    /// </summary>
    /// <param name="request">Login credentials containing the user's email and password.</param>
    /// <returns>A <see cref="LoginResponse"/> containing the JWT, refresh token, token expiration, and the authenticated user's public data.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid or the account is locked.</exception>
    /// <remarks>
    /// On successful authentication this method updates the user's LastLoginAt timestamp and stores the generated refresh token on the user record.
    /// </remarks>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException(result.IsLockedOut ? "Account locked" : "Invalid credentials");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = await GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

        // Store refresh token
        user.SecurityStamp = refreshToken;
        await _userManager.UpdateAsync(user);

        var userResponse = await MapToUserResponse(user);

        return new LoginResponse(token, refreshToken, expiresAt, userResponse);
    }

    /// <summary>
    /// Refreshes authentication credentials by validating an expired JWT and the provided refresh token, then issues a new JWT and refresh token.
    /// </summary>
    /// <param name="request">Request containing the expired JWT in <c>Token</c> and the associated refresh token in <c>RefreshToken</c>.</param>
    /// <returns>A <see cref="LoginResponse"/> containing the new JWT, the new refresh token, its UTC expiration time, and the user's public data.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the expired token is invalid or the refresh token does not match the user's stored refresh token.</exception>
    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null || user.SecurityStamp != request.RefreshToken)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var newToken = await GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

        user.SecurityStamp = newRefreshToken;
        await _userManager.UpdateAsync(user);

        var userResponse = await MapToUserResponse(user);

        return new LoginResponse(newToken, newRefreshToken, expiresAt, userResponse);
    }

    /// <summary>
    /// Revokes any existing refresh tokens for the specified user by resetting their security stamp and persisting the change.
    /// </summary>
    /// <param name="userId">The identifier of the user whose tokens should be revoked.</param>
    /// <exception cref="KeyNotFoundException">Thrown if no user with the specified id exists.</exception>
    public async Task RevokeTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        user.SecurityStamp = Guid.NewGuid().ToString();
        await _userManager.UpdateAsync(user);
    }

    /// <summary>
    /// Retrieves a user's public information by their identifier.
    /// </summary>
    /// <param name="userId">The identifier of the user to retrieve.</param>
    /// <returns>A <see cref="UserResponse"/> representing the user.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no user exists with the specified id.</exception>
    public async Task<UserResponse> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return await MapToUserResponse(user);
    }

    /// <summary>
    /// Updates a user's first and/or last name and returns the updated user representation.
    /// </summary>
    /// <param name="userId">The identifier of the user to update.</param>
    /// <param name="request">The update payload; non-empty <c>FirstName</c> and <c>LastName</c> values will be applied.</param>
    /// <returns>The updated user mapped to a <see cref="UserResponse"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no user exists with the specified <paramref name="userId"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the user update operation fails; the exception message contains the identity error descriptions.</exception>
    public async Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!string.IsNullOrEmpty(request.FirstName)) user.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName)) user.LastName = request.LastName;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return await MapToUserResponse(user);
    }

    /// <summary>
    /// Changes the password for the user identified by <paramref name="userId"/> using the provided current and new passwords.
    /// </summary>
    /// <param name="userId">The identifier of the user whose password will be changed.</param>
    /// <param name="request">Contains the current password and the new password to set.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no user exists with the specified <paramref name="userId"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the password change fails; the exception message contains concatenated identity error descriptions.</exception>
    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    /// <summary>
    /// Assigns the specified role to the user identified in the request.
    /// </summary>
    /// <param name="request">Contains the target user's ID and the role name to assign.</param>
    /// <exception cref="KeyNotFoundException">Thrown if no user exists with the provided ID.</exception>
    /// <exception cref="ArgumentException">Thrown if the specified role does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the role assignment fails; the exception message contains identity error descriptions.</exception>
    public async Task AssignRoleAsync(AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!await _roleManager.RoleExistsAsync(request.RoleName))
        {
            throw new ArgumentException("Role does not exist");
        }

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    /// <summary>
    /// Builds a JSON Web Token that encodes the specified user's identity and roles.
    /// </summary>
    /// <param name="user">The user whose identity and roles will be embedded as claims in the token.</param>
    /// <returns>The serialized JWT string containing the user's id, email, name, user type, and role claims, signed with the configured key and set to expire per configuration.</returns>
    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("UserType", user.UserType.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates a cryptographically secure random token and encodes it as Base64.
    /// </summary>
    /// <returns>A Base64-encoded string containing 32 cryptographically secure random bytes suitable for use as a refresh token.</returns>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Extracts the claims principal from a JWT even if the token has expired.
    /// </summary>
    /// <param name="token">The JWT string to validate and read (may be expired).</param>
    /// <returns>
    /// A <see cref="ClaimsPrincipal"/> containing the token's claims if the token is valid and signed with the service's signing key using HmacSha256; otherwise <c>null</c>.
    /// </returns>
    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Maps an ApplicationUser to a UserResponse DTO including the user's roles and active status.
    /// </summary>
    /// <param name="user">The application user to map.</param>
    /// <returns>A UserResponse containing the user's id, email, first name, last name, user type, roles, and active flag.</returns>
    private async Task<UserResponse> MapToUserResponse(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        
        return new UserResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.UserType.ToString(),
            roles.ToArray(),
            user.IsActive
        );
    }
}