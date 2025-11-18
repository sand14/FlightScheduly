using Auth.Api.Data;
using FluentAssertions;

namespace Auth.Api.Tests.Data;

public class UserRoleTests
{
    [Fact]
    public void UserRole_ShouldInitializeWithDefaultValues()
    {
        // Act
        var userRole = new UserRole
        {
            UserId = string.Empty,
            RoleId = string.Empty,
        };

        // Assert
        userRole.UserId.Should().Be(string.Empty);
        userRole.RoleId.Should().Be(string.Empty);
        userRole.AssignedAt.Should().Be(default(DateTime));
        userRole.User.Should().BeNull();
        userRole.Role.Should().BeNull();
    }

    [Fact]
    public void UserRole_ShouldAllowSettingUserId()
    {
        // Arrange
        var userRole = new UserRole
        {
            UserId = "user-id",
            RoleId = "role-id",
        };

        // Act
        userRole.UserId = "test-user-id";

        // Assert
        userRole.UserId.Should().Be("test-user-id");
    }

    [Fact]
    public void UserRole_ShouldAllowSettingRoleId()
    {
        // Arrange
        var userRole = new UserRole
        {
            UserId = "user-id",
            RoleId = "role-id",
        };

        // Act
        userRole.RoleId = "test-role-id";

        // Assert
        userRole.RoleId.Should().Be("test-role-id");
    }

    [Fact]
    public void UserRole_ShouldAllowSettingAssignedAt()
    {
        // Arrange
        var userRole = new UserRole
        {
            UserId = "user-id",
            RoleId = "role-id",
        };
        var now = DateTime.UtcNow;

        // Act
        userRole.AssignedAt = now;

        // Assert
        userRole.AssignedAt.Should().Be(now);
    }

    [Fact]
    public void UserRole_ShouldAllowSettingUser()
    {
        // Arrange
        var userRole = new UserRole
        {
            UserId = "user-id",
            RoleId = "role-id",
        };
        var user = new ApplicationUser { Id = "user-id" };

        // Act
        userRole.User = user;

        // Assert
        userRole.User.Should().BeSameAs(user);
    }

    [Fact]
    public void UserRole_ShouldAllowSettingRole()
    {
        // Arrange
        var userRole = new UserRole
        {
            UserId = "user-id",
            RoleId = "role-id",
        };
        var role = new ApplicationRole { Id = "role-id" };

        // Act
        userRole.Role = role;

        // Assert
        userRole.Role.Should().BeSameAs(role);
    }

    [Fact]
    public void UserRole_ShouldAllowCompleteInitialization()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var user = new ApplicationUser { Id = "user-id" };
        var role = new ApplicationRole { Id = "role-id" };

        // Act
        var userRole = new UserRole
        {
            UserId = "user-id",
            RoleId = "role-id",
            AssignedAt = now,
            User = user,
            Role = role
        };

        // Assert
        userRole.UserId.Should().Be("user-id");
        userRole.RoleId.Should().Be("role-id");
        userRole.AssignedAt.Should().Be(now);
        userRole.User.Should().BeSameAs(user);
        userRole.Role.Should().BeSameAs(role);
    }

    [Fact]
    public void UserRole_ShouldSupportNavigationProperties()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-id",
            FirstName = "John",
            LastName = "Doe"
        };
        var role = new ApplicationRole
        {
            Id = "role-id",
            Name = "Administrator"
        };

        // Act
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            User = user,
            Role = role,
            AssignedAt = DateTime.UtcNow
        };

        // Assert
        userRole.User.FirstName.Should().Be("John");
        userRole.User.LastName.Should().Be("Doe");
        userRole.Role.Name.Should().Be("Administrator");
    }

    [Fact]
    public void UserRole_ShouldAllowNullNavigationProperties()
    {
        // Arrange
        var userRole = new UserRole
        {
            UserId = "user-id",
            RoleId = "role-id",
            AssignedAt = DateTime.UtcNow
        };

        // Assert
        userRole.User.Should().BeNull();
        userRole.Role.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("short-id")]
    [InlineData("very-long-user-id-with-many-characters-12345")]
    public void UserRole_ShouldAcceptVariousUserIdFormats(string userId)
    {
        // Arrange
        var userRole = new UserRole() 
        { 
            RoleId = "role-id",
            UserId = userId,
        };

        // Assert
        userRole.UserId.Should().Be(userId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short-id")]
    [InlineData("very-long-role-id-with-many-characters-12345")]
    public void UserRole_ShouldAcceptVariousRoleIdFormats(string roleId)
    {
        // Arrange
        var userRole = new UserRole()
        {
            RoleId = roleId,
            UserId = "userId",
        };

        // Assert
        userRole.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void UserRole_AssignedAtShouldPreservePrecision()
    {
        // Arrange
        var preciseTime = new DateTime(2024, 1, 15, 10, 30, 45, 123, DateTimeKind.Utc);
        var userRole = new UserRole()
        {
            RoleId = "role-id",
            UserId = "user-id",
        };

        // Act
        userRole.AssignedAt = preciseTime;

        // Assert
        userRole.AssignedAt.Should().Be(preciseTime);
        userRole.AssignedAt.Millisecond.Should().Be(123);
    }
}