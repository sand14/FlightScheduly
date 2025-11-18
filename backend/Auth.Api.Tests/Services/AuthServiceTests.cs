using Auth.Api.Configuration;
using Auth.Api.Constants;
using Auth.Api.Data;
using Auth.Api.Models;
using Auth.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace Auth.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
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
            Secret = "this-is-a-very-long-secret-key-for-testing-purposes-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };
        _jwtSettings = Options.Create(jwtSettings);

        _authService = new AuthService(_userManagerMock.Object, _signInManagerMock.Object, _roleManagerMock.Object, _jwtSettings);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterRequest(
            "test@example.com",
            "Password123!",
            "John",
            "Doe"
        );

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock.Setup(x => x.RoleExistsAsync(Roles.Student))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Student))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { Roles.Student });

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        _userManagerMock.Verify(x => x.CreateAsync(
            It.Is<ApplicationUser>(u =>
                u.Email == request.Email &&
                u.FirstName == request.FirstName &&
                u.LastName == request.LastName &&
                u.UserType == UserType.Student),
            request.Password), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidData_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest(
            "invalid-email",
            "Password123!",
            "John",
            "Doe"
        );

        var errors = new[] { new IdentityError { Description = "Invalid email format" } };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var action = async () => await _authService.RegisterAsync(request);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid email format*");
    }

    [Fact]
    public async Task RegisterAsync_ShouldAssignStudentRole()
    {
        // Arrange
        var request = new RegisterRequest(
            "test@example.com",
            "Password123!",
            "John",
            "Doe"
        );

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock.Setup(x => x.RoleExistsAsync(Roles.Student))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Student))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { Roles.Student });

        // Act
        await _authService.RegisterAsync(request);

        // Assert
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Student), Times.Once);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!");
        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { Roles.Student });

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@example.com", "Password123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var action = async () => await _authService.LoginAsync(request);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!");
        var user = new ApplicationUser
        {
            Email = "test@example.com",
            IsActive = false
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var action = async () => await _authService.LoginAsync(request);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "WrongPassword!");
        var user = new ApplicationUser
        {
            Email = "test@example.com",
            IsActive = true
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var action = async () => await _authService.LoginAsync(request);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!");
        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            LastLoginAt = null
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { Roles.Student });

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _authService.LoginAsync(request);

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Exactly(2)); // Once for LastLoginAt, once for SecurityStamp
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = "user-id";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserType.Student,
            IsActive = true
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { Roles.Student });

        // Act
        var result = await _authService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be(user.Email);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = "nonexistent-id";

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var action = async () => await _authService.GetUserByIdAsync(userId);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var userId = "user-id";
        var user = new ApplicationUser
        {
            Id = userId,
            FirstName = "OldFirst",
            LastName = "OldLast",
            IsActive = true
        };

        var updateRequest = new UpdateUserRequest(
            "NewFirst",
            "NewLast",
            null,
            null
        );

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { Roles.Student });

        // Act
        var result = await _authService.UpdateUserAsync(userId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        user.FirstName.Should().Be("NewFirst");
        user.LastName.Should().Be("NewLast");
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = "nonexistent-id";
        var updateRequest = new UpdateUserRequest(
            "First",
            "Last",
            null,
            null
        );

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var action = async () => await _authService.UpdateUserAsync(userId, updateRequest);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithValidData_ShouldChangePassword()
    {
        // Arrange
        var userId = "user-id";
        var request = new ChangePasswordRequest("OldPassword123!", "NewPassword456!");
        var user = new ApplicationUser { Id = userId };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _authService.ChangePasswordAsync(userId, request);

        // Assert
        _userManagerMock.Verify(x => x.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = "nonexistent-id";
        var request = new ChangePasswordRequest("OldPassword123!", "NewPassword456!");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var action = async () => await _authService.ChangePasswordAsync(userId, request);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = "user-id";
        var request = new ChangePasswordRequest("WrongPassword!", "NewPassword456!");
        var user = new ApplicationUser { Id = userId };
        var errors = new[] { new IdentityError { Description = "Incorrect password" } };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var action = async () => await _authService.ChangePasswordAsync(userId, request);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Incorrect password*");
    }

    #endregion

    #region AssignRoleAsync Tests

    [Fact]
    public async Task AssignRoleAsync_WithValidData_ShouldAssignRole()
    {
        // Arrange
        var request = new AssignRoleRequest("user-id", Roles.Instructor);
        var user = new ApplicationUser { Id = request.UserId };

        _userManagerMock.Setup(x => x.FindByIdAsync(request.UserId))
            .ReturnsAsync(user);

        _roleManagerMock.Setup(x => x.RoleExistsAsync(request.RoleName))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.AddToRoleAsync(user, request.RoleName))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _authService.AssignRoleAsync(request);

        // Assert
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, request.RoleName), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var request = new AssignRoleRequest("nonexistent-id", Roles.Student);

        _userManagerMock.Setup(x => x.FindByIdAsync(request.UserId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var action = async () => await _authService.AssignRoleAsync(request);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task AssignRoleAsync_WithNonExistentRole_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new AssignRoleRequest("user-id", "NonExistentRole");
        var user = new ApplicationUser { Id = request.UserId };

        _userManagerMock.Setup(x => x.FindByIdAsync(request.UserId))
            .ReturnsAsync(user);

        _roleManagerMock.Setup(x => x.RoleExistsAsync(request.RoleName))
            .ReturnsAsync(false);

        // Act
        var action = async () => await _authService.AssignRoleAsync(request);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Role does not exist");
    }

    #endregion

    #region RevokeTokenAsync Tests

    [Fact]
    public async Task RevokeTokenAsync_WithValidUserId_ShouldRevokeToken()
    {
        // Arrange
        var userId = "user-id";
        var user = new ApplicationUser { Id = userId, SecurityStamp = "old-stamp" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _authService.RevokeTokenAsync(userId);

        // Assert
        user.SecurityStamp.Should().NotBe("old-stamp");
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = "nonexistent-id";

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var action = async () => await _authService.RevokeTokenAsync(userId);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    #endregion
}