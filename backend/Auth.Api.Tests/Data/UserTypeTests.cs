using Auth.Api.Data;
using FluentAssertions;

namespace Auth.Api.Tests.Data;

public class UserTypeTests
{
    [Fact]
    public void UserType_ShouldHaveInstructorValue()
    {
        // Assert
        UserType.Instructor.Should().BeDefined();
        ((int)UserType.Instructor).Should().Be(1);
    }

    [Fact]
    public void UserType_ShouldHaveStudentValue()
    {
        // Assert
        UserType.Student.Should().BeDefined();
        ((int)UserType.Student).Should().Be(2);
    }

    [Fact]
    public void UserType_ShouldHavePilotValue()
    {
        // Assert
        UserType.Pilot.Should().BeDefined();
        ((int)UserType.Pilot).Should().Be(3);
    }

    [Fact]
    public void UserType_ShouldHaveAdministratorValue()
    {
        // Assert
        UserType.Administrator.Should().BeDefined();
        ((int)UserType.Administrator).Should().Be(4);
    }

    [Fact]
    public void UserType_ShouldHaveUniqueIntegerValues()
    {
        // Arrange
        var values = Enum.GetValues<UserType>().Cast<int>();

        // Assert
        values.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(UserType.Instructor, "Instructor")]
    [InlineData(UserType.Student, "Student")]
    [InlineData(UserType.Pilot, "Pilot")]
    [InlineData(UserType.Administrator, "Administrator")]
    public void UserType_ShouldHaveCorrectStringRepresentation(UserType userType, string expected)
    {
        // Act
        var result = userType.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void UserType_ShouldBeConvertibleToInt()
    {
        // Assert
        ((int)UserType.Instructor).Should().BeOfType<int>();
        ((int)UserType.Student).Should().BeOfType<int>();
        ((int)UserType.Pilot).Should().BeOfType<int>();
        ((int)UserType.Administrator).Should().BeOfType<int>();
    }

    [Theory]
    [InlineData(1, UserType.Instructor)]
    [InlineData(2, UserType.Student)]
    [InlineData(3, UserType.Pilot)]
    [InlineData(4, UserType.Administrator)]
    public void UserType_ShouldBeConvertibleFromInt(int value, UserType expected)
    {
        // Act
        var result = (UserType)value;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void UserType_ShouldHaveExactlyFourValues()
    {
        // Arrange
        var values = Enum.GetValues<UserType>();

        // Assert
        values.Length.Should().Be(4);
    }

    [Theory]
    [InlineData("Instructor", UserType.Instructor)]
    [InlineData("Student", UserType.Student)]
    [InlineData("Pilot", UserType.Pilot)]
    [InlineData("Administrator", UserType.Administrator)]
    public void UserType_ShouldBeParsableFromString(string input, UserType expected)
    {
        // Act
        var result = Enum.Parse<UserType>(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("instructor")]
    [InlineData("STUDENT")]
    [InlineData("pIlOt")]
    public void UserType_ShouldBeParsableFromStringIgnoringCase(string input)
    {
        // Act
        var result = Enum.Parse<UserType>(input, ignoreCase: true);

        // Assert
        result.Should().BeDefined();
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("")]
    [InlineData("123")]
    public void UserType_ShouldThrowExceptionForInvalidString(string input)
    {
        // Act
        var action = () => Enum.Parse<UserType>(input);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Instructor", true)]
    [InlineData("Student", true)]
    [InlineData("Pilot", true)]
    [InlineData("Administrator", true)]
    [InlineData("Invalid", false)]
    public void UserType_ShouldValidateIfDefinedValue(string input, bool shouldBeDefined)
    {
        // Act
        var parseSuccess = Enum.TryParse<UserType>(input, out var result);

        // Assert
        if (shouldBeDefined)
        {
            parseSuccess.Should().BeTrue();
            result.Should().BeDefined();
        }
        else
        {
            parseSuccess.Should().BeFalse();
        }
    }
}