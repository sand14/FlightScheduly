using Auth.Api.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Tests.Data;

public class ApplicationRoleTests
{
    [Fact]
    public void ApplicationRole_ShouldInitializeWithDefaultValues()
    {
        // Act
        var role = new ApplicationRole();

        // Assert
        role.Description.Should().Be(string.Empty);
        role.UserRoles.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ApplicationRole_ShouldAllowSettingDescription()
    {
        // Arrange
        var role = new ApplicationRole();

        // Act
        role.Description = "Test description";

        // Assert
        role.Description.Should().Be("Test description");
    }

    [Fact]
    public void ApplicationRole_ShouldInheritFromIdentityRole()
    {
        // Arrange
        var role = new ApplicationRole();

        // Assert
        role.Should().BeAssignableTo<IdentityRole>();
    }

    [Fact]
    public void ApplicationRole_ShouldHaveIdentityRoleProperties()
    {
        // Arrange
        var role = new ApplicationRole
        {
            Name = "TestRole",
            NormalizedName = "TESTROLE"
        };

        // Assert
        role.Name.Should().Be("TestRole");
        role.NormalizedName.Should().Be("TESTROLE");
    }

    [Fact]
    public void ApplicationRole_ShouldInitializeUserRolesCollection()
    {
        // Act
        var role = new ApplicationRole();

        // Assert
        role.UserRoles.Should().NotBeNull();
        role.UserRoles.Should().BeAssignableTo<ICollection<UserRole>>();
    }

    [Fact]
    public void ApplicationRole_ShouldAllowAddingUserRoles()
    {
        // Arrange
        var role = new ApplicationRole();
        var userRole = new UserRole
        {
            UserId = "user-id",
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        };

        // Act
        role.UserRoles.Add(userRole);

        // Assert
        role.UserRoles.Should().HaveCount(1);
        role.UserRoles.Should().Contain(userRole);
    }

    [Fact]
    public void ApplicationRole_ShouldAllowCompleteInitialization()
    {
        // Act
        var role = new ApplicationRole
        {
            Id = "role-id",
            Name = "Administrator",
            NormalizedName = "ADMINISTRATOR",
            Description = "Full system access",
            ConcurrencyStamp = "test-stamp"
        };

        // Assert
        role.Id.Should().Be("role-id");
        role.Name.Should().Be("Administrator");
        role.NormalizedName.Should().Be("ADMINISTRATOR");
        role.Description.Should().Be("Full system access");
        role.ConcurrencyStamp.Should().Be("test-stamp");
    }

    [Fact]
    public void ApplicationRole_DescriptionShouldSupportEmptyString()
    {
        // Arrange
        var role = new ApplicationRole
        {
            Description = ""
        };

        // Assert
        role.Description.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Short")]
    [InlineData("This is a much longer description that spans multiple words")]
    [InlineData("Description with special characters: !@#$%^&*()")]
    public void ApplicationRole_ShouldAcceptVariousDescriptions(string description)
    {
        // Arrange
        var role = new ApplicationRole();

        // Act
        role.Description = description;

        // Assert
        role.Description.Should().Be(description);
    }

    [Fact]
    public void ApplicationRole_ShouldAllowMultipleUserRoles()
    {
        // Arrange
        var role = new ApplicationRole();
        var userRole1 = new UserRole { UserId = "user1", RoleId = role.Id };
        var userRole2 = new UserRole { UserId = "user2", RoleId = role.Id };
        var userRole3 = new UserRole { UserId = "user3", RoleId = role.Id };

        // Act
        role.UserRoles.Add(userRole1);
        role.UserRoles.Add(userRole2);
        role.UserRoles.Add(userRole3);

        // Assert
        role.UserRoles.Should().HaveCount(3);
        role.UserRoles.Should().Contain(new[] { userRole1, userRole2, userRole3 });
    }
}