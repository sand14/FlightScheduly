using Auth.Api.Constants;
using FluentAssertions;

namespace Auth.Api.Tests.Constants;

public class RolesTests
{
    [Fact]
    public void Roles_ShouldHaveInstructorConstant()
    {
        // Assert
        Roles.Instructor.Should().Be("Instructor");
    }

    [Fact]
    public void Roles_ShouldHaveStudentConstant()
    {
        // Assert
        Roles.Student.Should().Be("Student");
    }

    [Fact]
    public void Roles_ShouldHavePilotConstant()
    {
        // Assert
        Roles.Pilot.Should().Be("Pilot");
    }

    [Fact]
    public void Roles_ShouldHaveAdministratorConstant()
    {
        // Assert
        Roles.Administrator.Should().Be("Administrator");
    }

    [Fact]
    public void Roles_ShouldHaveUniqueValues()
    {
        // Arrange
        var roles = new[]
        {
            Roles.Instructor,
            Roles.Student,
            Roles.Pilot,
            Roles.Administrator
        };

        // Assert
        roles.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Roles_ShouldNotBeNullOrEmpty()
    {
        // Assert
        Roles.Instructor.Should().NotBeNullOrEmpty();
        Roles.Student.Should().NotBeNullOrEmpty();
        Roles.Pilot.Should().NotBeNullOrEmpty();
        Roles.Administrator.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Roles_ShouldNotContainWhitespace()
    {
        // Assert
        Roles.Instructor.Should().NotContain(" ");
        Roles.Student.Should().NotContain(" ");
        Roles.Pilot.Should().NotContain(" ");
        Roles.Administrator.Should().NotContain(" ");
    }

    [Theory]
    [InlineData("Instructor")]
    [InlineData("Student")]
    [InlineData("Pilot")]
    [InlineData("Administrator")]
    public void Roles_ShouldMatchExpectedValues(string expectedRole)
    {
        // Arrange
        var roles = new[]
        {
            Roles.Instructor,
            Roles.Student,
            Roles.Pilot,
            Roles.Administrator
        };

        // Assert
        roles.Should().Contain(expectedRole);
    }

    [Fact]
    public void Roles_ConstantsShouldBeAccessibleWithoutInstantiation()
    {
        // This test verifies that Roles is a static class
        // If this compiles, the test passes
        var instructor = Roles.Instructor;
        var student = Roles.Student;
        var pilot = Roles.Pilot;
        var administrator = Roles.Administrator;

        // Assert
        instructor.Should().NotBeNull();
        student.Should().NotBeNull();
        pilot.Should().NotBeNull();
        administrator.Should().NotBeNull();
    }

    [Fact]
    public void Roles_ShouldHaveExactlyFourRoles()
    {
        // Arrange
        var roleType = typeof(Roles);
        var constants = roleType.GetFields(System.Reflection.BindingFlags.Public | 
                                           System.Reflection.BindingFlags.Static | 
                                           System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .ToList();

        // Assert
        constants.Should().HaveCount(4);
    }
}