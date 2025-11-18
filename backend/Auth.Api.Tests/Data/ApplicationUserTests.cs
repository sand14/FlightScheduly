using Auth.Api.Data;
using FluentAssertions;

namespace Auth.Api.Tests.Data;

public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        user.FirstName.Should().Be(string.Empty);
        user.LastName.Should().Be(string.Empty);
        user.UserType.Should().Be(default(UserType));
        user.CreatedAt.Should().Be(default(DateTime));
        user.LastLoginAt.Should().BeNull();
        user.LicenseExpirationDate.Should().BeNull();
        user.RadioLicenseExpirationDate.Should().BeNull();
        user.MedicalLicenseExpirationDate.Should().BeNull();
        user.IsActive.Should().BeTrue();
        user.UserRoles.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ApplicationUser_ShouldAllowSettingFirstName()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.FirstName = "John";

        // Assert
        user.FirstName.Should().Be("John");
    }

    [Fact]
    public void ApplicationUser_ShouldAllowSettingLastName()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.LastName = "Doe";

        // Assert
        user.LastName.Should().Be("Doe");
    }

    [Theory]
    [InlineData(UserType.Instructor)]
    [InlineData(UserType.Student)]
    [InlineData(UserType.Pilot)]
    [InlineData(UserType.Administrator)]
    public void ApplicationUser_ShouldAllowSettingUserType(UserType userType)
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.UserType = userType;

        // Assert
        user.UserType.Should().Be(userType);
    }

    [Fact]
    public void ApplicationUser_ShouldAllowSettingCreatedAt()
    {
        // Arrange
        var user = new ApplicationUser();
        var now = DateTime.UtcNow;

        // Act
        user.CreatedAt = now;

        // Assert
        user.CreatedAt.Should().Be(now);
    }

    [Fact]
    public void ApplicationUser_ShouldAllowSettingLastLoginAt()
    {
        // Arrange
        var user = new ApplicationUser();
        var loginTime = DateTime.UtcNow;

        // Act
        user.LastLoginAt = loginTime;

        // Assert
        user.LastLoginAt.Should().Be(loginTime);
    }

    [Fact]
    public void ApplicationUser_ShouldAllowNullLastLoginAt()
    {
        // Arrange
        var user = new ApplicationUser
        {
            LastLoginAt = DateTime.UtcNow
        };

        // Act
        user.LastLoginAt = null;

        // Assert
        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void ApplicationUser_ShouldAllowSettingLicenseExpirationDate()
    {
        // Arrange
        var user = new ApplicationUser();
        var expirationDate = DateTime.UtcNow.AddYears(1);

        // Act
        user.LicenseExpirationDate = expirationDate;

        // Assert
        user.LicenseExpirationDate.Should().Be(expirationDate);
    }

    [Fact]
    public void ApplicationUser_ShouldAllowSettingRadioLicenseExpirationDate()
    {
        // Arrange
        var user = new ApplicationUser();
        var expirationDate = DateTime.UtcNow.AddMonths(6);

        // Act
        user.RadioLicenseExpirationDate = expirationDate;

        // Assert
        user.RadioLicenseExpirationDate.Should().Be(expirationDate);
    }

    [Fact]
    public void ApplicationUser_ShouldAllowSettingMedicalLicenseExpirationDate()
    {
        // Arrange
        var user = new ApplicationUser();
        var expirationDate = DateTime.UtcNow.AddMonths(12);

        // Act
        user.MedicalLicenseExpirationDate = expirationDate;

        // Assert
        user.MedicalLicenseExpirationDate.Should().Be(expirationDate);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplicationUser_ShouldAllowSettingIsActive(bool isActive)
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.IsActive = isActive;

        // Assert
        user.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void ApplicationUser_ShouldInitializeUserRolesCollection()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        user.UserRoles.Should().NotBeNull();
        user.UserRoles.Should().BeAssignableTo<ICollection<UserRole>>();
    }

    [Fact]
    public void ApplicationUser_ShouldAllowAddingUserRoles()
    {
        // Arrange
        var user = new ApplicationUser();
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = "role-id",
            AssignedAt = DateTime.UtcNow
        };

        // Act
        user.UserRoles.Add(userRole);

        // Assert
        user.UserRoles.Should().HaveCount(1);
        user.UserRoles.Should().Contain(userRole);
    }

    [Fact]
    public void ApplicationUser_ShouldInheritFromIdentityUser()
    {
        // Arrange
        var user = new ApplicationUser();

        // Assert
        user.Should().BeAssignableTo<Microsoft.AspNetCore.Identity.IdentityUser>();
    }

    [Fact]
    public void ApplicationUser_ShouldHaveIdentityUserProperties()
    {
        // Arrange
        var user = new ApplicationUser
        {
            UserName = "testuser",
            Email = "test@example.com",
            PhoneNumber = "1234567890"
        };

        // Assert
        user.UserName.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.PhoneNumber.Should().Be("1234567890");
    }

    [Fact]
    public void ApplicationUser_ShouldAllowCompleteInitialization()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var licenseExpiry = now.AddYears(2);
        var radioExpiry = now.AddYears(1);
        var medicalExpiry = now.AddMonths(6);

        // Act
        var user = new ApplicationUser
        {
            Id = "user-id",
            UserName = "jdoe",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            UserType = UserType.Pilot,
            CreatedAt = now,
            LastLoginAt = now,
            LicenseExpirationDate = licenseExpiry,
            RadioLicenseExpirationDate = radioExpiry,
            MedicalLicenseExpirationDate = medicalExpiry,
            IsActive = true
        };

        // Assert
        user.Id.Should().Be("user-id");
        user.UserName.Should().Be("jdoe");
        user.Email.Should().Be("john.doe@example.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.UserType.Should().Be(UserType.Pilot);
        user.CreatedAt.Should().Be(now);
        user.LastLoginAt.Should().Be(now);
        user.LicenseExpirationDate.Should().Be(licenseExpiry);
        user.RadioLicenseExpirationDate.Should().Be(radioExpiry);
        user.MedicalLicenseExpirationDate.Should().Be(medicalExpiry);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ApplicationUser_ShouldSupportEmptyStringNames()
    {
        // Arrange
        var user = new ApplicationUser
        {
            FirstName = "",
            LastName = ""
        };

        // Assert
        user.FirstName.Should().BeEmpty();
        user.LastName.Should().BeEmpty();
    }

    [Fact]
    public void ApplicationUser_LicenseDatesShouldBeNullableByDefault()
    {
        // Arrange
        var user = new ApplicationUser();

        // Assert
        user.LicenseExpirationDate.Should().BeNull();
        user.RadioLicenseExpirationDate.Should().BeNull();
        user.MedicalLicenseExpirationDate.Should().BeNull();
        user.LastLoginAt.Should().BeNull();
    }
}