using Auth.Api.Configuration;
using Auth.Api.Constants;
using Auth.Api.Data;
using Auth.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Api.Tests.Services;

/// <summary>
/// Tests specifically for JWT token generation and validation logic
/// </summary>
public class JwtTokenTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly AuthService _authService;

    public JwtTokenTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null, null, null, null, null, null, null, null);

        var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            contextAccessorMock.Object,
            userPrincipalFactoryMock.Object,
            null, null, null, null);

        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object,
            null, null, null, null);

        var jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-very-long-secret-key-for-testing-purposes-minimum-32-characters-required-for-security",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };
        _jwtSettings = Options.Create(jwtSettings);

        _authService = new AuthService(_userManagerMock.Object, _signInManagerMock.Object, _roleManagerMock.Object, _jwtSettings);
    }

    [Fact]
    public async Task GeneratedToken_ShouldHaveCorrectFormat()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        var token = result.Token;
        token.Should().NotBeNullOrEmpty();
        
        // JWT tokens have 3 parts separated by dots
        var parts = token.Split('.');
        parts.Should().HaveCount(3);
    }

    [Fact]
    public async Task GeneratedToken_ShouldContainUserId()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        var token = result.Token;
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be(user.Id);
    }

    [Fact]
    public async Task GeneratedToken_ShouldContainEmail()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        var token = result.Token;
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(user.Email);
    }

    [Fact]
    public async Task GeneratedToken_ShouldContainRoles()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");
        var roles = new List<string> { Roles.Student, Roles.Pilot };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        var token = result.Token;
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Should().HaveCount(2);
        roleClaims.Select(c => c.Value).Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task GeneratedToken_ShouldHaveCorrectIssuer()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        var token = result.Token;
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be(_jwtSettings.Value.Issuer);
    }

    [Fact]
    public async Task GeneratedToken_ShouldHaveCorrectAudience()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        var token = result.Token;
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Audiences.Should().Contain(_jwtSettings.Value.Audience);
    }

    [Fact]
    public async Task GeneratedToken_ShouldHaveCorrectExpiration()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        var token = result.Token;
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpirationInMinutes);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task RefreshToken_ShouldNotBeEmpty()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_ShouldBeDifferentFromAccessToken()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.RefreshToken.Should().NotBe(result.Token);
    }

    [Fact]
    public async Task RefreshToken_ShouldHaveSufficientLength()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        // Refresh tokens should be sufficiently long for security (Base64-encoded 32 bytes = 44 chars)
        result.RefreshToken.Length.Should().BeGreaterThan(32);
    }

    [Fact]
    public async Task GeneratedToken_ShouldContainUserTypeClaim()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        var token = result.Token;
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserType");
        userTypeClaim.Should().NotBeNull();
        userTypeClaim!.Value.Should().Be(user.UserType.ToString());
    }

    [Fact]
    public async Task GeneratedToken_ShouldContainNameClaim()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new Models.LoginRequest(user.Email!, "Password123!");

        SetupSuccessfulLogin(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        var token = result.Token;
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        nameClaim.Should().NotBeNull();
        nameClaim!.Value.Should().Be($"{user.FirstName} {user.LastName}");
    }

    // Helper methods
    private ApplicationUser CreateTestUser(string id = "user-id", string email = "test@example.com")
    {
        return new ApplicationUser
        {
            Id = id,
            Email = email,
            UserName = email,
            FirstName = "John",
            LastName = "Doe",
            UserType = UserType.Student,
            IsActive = true
        };
    }

    private void SetupSuccessfulLogin(ApplicationUser user)
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, It.IsAny<string>(), true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { Roles.Student });

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
    }
}