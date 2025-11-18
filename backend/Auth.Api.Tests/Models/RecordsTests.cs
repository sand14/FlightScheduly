using Auth.Api.Models;
using FluentAssertions;

namespace Auth.Api.Tests.Models;

public class RegisterRequestTests
{
    [Fact]
    public void RegisterRequest_ShouldInitializeWithRequiredProperties()
    {
        // Act
        var request = new RegisterRequest("john.doe@example.com", "Password123!", "John", "Doe");

        // Assert
        request.Email.Should().Be("john.doe@example.com");
        request.Password.Should().Be("Password123!");
        request.FirstName.Should().Be("John");
        request.LastName.Should().Be("Doe");
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("first.last+tag@example.com")]
    public void RegisterRequest_ShouldAcceptValidEmails(string email)
    {
        // Act
        var request = new RegisterRequest(email, "Pass123!", "First", "Last");

        // Assert
        request.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("Simple123!")]
    [InlineData("C0mpl3x!P@ssw0rd")]
    [InlineData("VeryLongPasswordWithSpecialChars123!@#$")]
    public void RegisterRequest_ShouldAcceptVariousPasswords(string password)
    {
        // Act
        var request = new RegisterRequest("test@example.com", password, "First", "Last");

        // Assert
        request.Password.Should().Be(password);
    }

    [Fact]
    public void RegisterRequest_ShouldBeImmutableRecord()
    {
        // Arrange
        var request1 = new RegisterRequest("test@example.com", "Pass123!", "John", "Doe");
        var request2 = new RegisterRequest("test@example.com", "Pass123!", "John", "Doe");

        // Assert
        request1.Should().Be(request2);
        request1.GetHashCode().Should().Be(request2.GetHashCode());
    }

    [Fact]
    public void RegisterRequest_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new RegisterRequest("test1@example.com", "Pass123!", "John", "Doe");
        var request2 = new RegisterRequest("test2@example.com", "Pass123!", "John", "Doe");

        // Assert
        request1.Should().NotBe(request2);
    }
}

public class LoginRequestTests
{
    [Fact]
    public void LoginRequest_ShouldInitializeWithRequiredProperties()
    {
        // Act
        var request = new LoginRequest("test@example.com", "Password123!");

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be("Password123!");
        request.RememberMe.Should().BeFalse();
    }

    [Fact]
    public void LoginRequest_ShouldSupportRememberMe()
    {
        // Act
        var request = new LoginRequest("test@example.com", "Password123!", true);

        // Assert
        request.RememberMe.Should().BeTrue();
    }

    [Theory]
    [InlineData("user@example.com", "Pass123!", false)]
    [InlineData("admin@domain.com", "AdminPass456$", true)]
    public void LoginRequest_ShouldAcceptValidCredentials(string email, string password, bool rememberMe)
    {
        // Act
        var request = new LoginRequest(email, password, rememberMe);

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
        request.RememberMe.Should().Be(rememberMe);
    }

    [Fact]
    public void LoginRequest_ShouldBeImmutableRecord()
    {
        // Arrange
        var request1 = new LoginRequest("test@example.com", "Pass123!");
        var request2 = new LoginRequest("test@example.com", "Pass123!");

        // Assert
        request1.Should().Be(request2);
    }
}

public class RefreshTokenRequestTests
{
    [Fact]
    public void RefreshTokenRequest_ShouldInitializeWithTokens()
    {
        // Act
        var request = new RefreshTokenRequest("access-token", "refresh-token");

        // Assert
        request.Token.Should().Be("access-token");
        request.RefreshToken.Should().Be("refresh-token");
    }

    [Theory]
    [InlineData("short", "token")]
    [InlineData("very-long-token-string", "very-long-refresh-token")]
    public void RefreshTokenRequest_ShouldAcceptVariousTokenFormats(string token, string refreshToken)
    {
        // Act
        var request = new RefreshTokenRequest(token, refreshToken);

        // Assert
        request.Token.Should().Be(token);
        request.RefreshToken.Should().Be(refreshToken);
    }
}

public class LoginResponseTests
{
    [Fact]
    public void LoginResponse_ShouldInitializeWithAllProperties()
    {
        // Arrange
        var userResponse = new UserResponse("id", "test@example.com", "John", "Doe", "Student", new[] { "Student" }, true);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var response = new LoginResponse("access-token", "refresh-token", expiresAt, userResponse);

        // Assert
        response.Token.Should().Be("access-token");
        response.RefreshToken.Should().Be("refresh-token");
        response.ExpiresAt.Should().Be(expiresAt);
        response.User.Should().Be(userResponse);
    }

    [Fact]
    public void LoginResponse_ShouldBeImmutableRecord()
    {
        // Arrange
        var user = new UserResponse("id", "test@example.com", "John", "Doe", "Student", new[] { "Student" }, true);
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var response1 = new LoginResponse("token", "refresh", expiresAt, user);
        var response2 = new LoginResponse("token", "refresh", expiresAt, user);

        // Assert
        response1.Should().Be(response2);
    }
}

public class UserResponseTests
{
    [Fact]
    public void UserResponse_ShouldInitializeWithAllProperties()
    {
        // Act
        var response = new UserResponse(
            "user-id",
            "test@example.com",
            "John",
            "Doe",
            "Student",
            new[] { "Student" },
            true
        );

        // Assert
        response.Id.Should().Be("user-id");
        response.Email.Should().Be("test@example.com");
        response.FirstName.Should().Be("John");
        response.LastName.Should().Be("Doe");
        response.UserType.Should().Be("Student");
        response.Roles.Should().BeEquivalentTo(new[] { "Student" });
        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UserResponse_ShouldSupportEmptyRoles()
    {
        // Act
        var response = new UserResponse(
            "user-id",
            "test@example.com",
            "John",
            "Doe",
            "Student",
            Array.Empty<string>(),
            true
        );

        // Assert
        response.Roles.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UserResponse_ShouldSupportIsActiveFlag(bool isActive)
    {
        // Act
        var response = new UserResponse(
            "user-id",
            "test@example.com",
            "John",
            "Doe",
            "Student",
            new[] { "Student" },
            isActive
        );

        // Assert
        response.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void UserResponse_ShouldSupportMultipleRoles()
    {
        // Arrange
        var roles = new[] { "Student", "Pilot", "Instructor" };

        // Act
        var response = new UserResponse(
            "user-id",
            "test@example.com",
            "John",
            "Doe",
            "Student",
            roles,
            true
        );

        // Assert
        response.Roles.Should().HaveCount(3);
        response.Roles.Should().BeEquivalentTo(roles);
    }
}

public class AssignRoleRequestTests
{
    [Fact]
    public void AssignRoleRequest_ShouldInitializeWithProperties()
    {
        // Act
        var request = new AssignRoleRequest("user-id", "Admin");

        // Assert
        request.UserId.Should().Be("user-id");
        request.RoleName.Should().Be("Admin");
    }

    [Theory]
    [InlineData("Instructor")]
    [InlineData("Student")]
    [InlineData("Pilot")]
    [InlineData("Administrator")]
    public void AssignRoleRequest_ShouldAcceptAllRoleNames(string roleName)
    {
        // Act
        var request = new AssignRoleRequest("user-id", roleName);

        // Assert
        request.RoleName.Should().Be(roleName);
    }
}

public class UpdateUserRequestTests
{
    [Fact]
    public void UpdateUserRequest_ShouldInitializeWithAllProperties()
    {
        // Arrange
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act
        var request = new UpdateUserRequest(
            "John",
            "Doe",
            dateOfBirth,
            "LIC123"
        );

        // Assert
        request.FirstName.Should().Be("John");
        request.LastName.Should().Be("Doe");
        request.DateOfBirth.Should().Be(dateOfBirth);
        request.LicenseNumber.Should().Be("LIC123");
    }

    [Fact]
    public void UpdateUserRequest_ShouldSupportNullValues()
    {
        // Act
        var request = new UpdateUserRequest(
            null,
            null,
            null,
            null
        );

        // Assert
        request.FirstName.Should().BeNull();
        request.LastName.Should().BeNull();
        request.DateOfBirth.Should().BeNull();
        request.LicenseNumber.Should().BeNull();
    }

    [Fact]
    public void UpdateUserRequest_ShouldSupportPartialUpdates()
    {
        // Act
        var request = new UpdateUserRequest(
            "John",
            null,
            null,
            "LIC123"
        );

        // Assert
        request.FirstName.Should().Be("John");
        request.LastName.Should().BeNull();
        request.DateOfBirth.Should().BeNull();
        request.LicenseNumber.Should().Be("LIC123");
    }
}

public class ChangePasswordRequestTests
{
    [Fact]
    public void ChangePasswordRequest_ShouldInitializeWithPasswords()
    {
        // Act
        var request = new ChangePasswordRequest("OldPass123!", "NewPass456!");

        // Assert
        request.CurrentPassword.Should().Be("OldPass123!");
        request.NewPassword.Should().Be("NewPass456!");
    }

    [Theory]
    [InlineData("Simple123!", "Complex456$")]
    [InlineData("OldPassword1!", "NewPassword2@")]
    public void ChangePasswordRequest_ShouldAcceptVariousPasswords(string current, string newPass)
    {
        // Act
        var request = new ChangePasswordRequest(current, newPass);

        // Assert
        request.CurrentPassword.Should().Be(current);
        request.NewPassword.Should().Be(newPass);
    }

    [Fact]
    public void ChangePasswordRequest_ShouldBeImmutableRecord()
    {
        // Arrange
        var request1 = new ChangePasswordRequest("Old123!", "New456!");
        var request2 = new ChangePasswordRequest("Old123!", "New456!");

        // Assert
        request1.Should().Be(request2);
    }
}