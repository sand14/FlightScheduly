using Auth.Api.Configuration;
using FluentAssertions;

namespace Auth.Api.Tests.Configuration;

public class JwtSettingsTests
{
    [Fact]
    public void JwtSettings_ShouldHaveRequiredSecret()
    {
        // Arrange & Act
        var action = () => new JwtSettings { Secret = null! };

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void JwtSettings_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var settings = new JwtSettings { Secret = "test-secret" };

        // Assert
        settings.Secret.Should().Be("test-secret");
        settings.Issuer.Should().Be(string.Empty);
        settings.Audience.Should().Be(string.Empty);
        settings.ExpirationInMinutes.Should().Be(60);
        settings.RefreshTokenExpirationInDays.Should().Be(7);
    }

    [Fact]
    public void JwtSettings_ShouldAllowCustomValues()
    {
        // Arrange & Act
        var settings = new JwtSettings
        {
            Secret = "custom-secret-key",
            Issuer = "https://test-issuer.com",
            Audience = "https://test-audience.com",
            ExpirationInMinutes = 120,
            RefreshTokenExpirationInDays = 14
        };

        // Assert
        settings.Secret.Should().Be("custom-secret-key");
        settings.Issuer.Should().Be("https://test-issuer.com");
        settings.Audience.Should().Be("https://test-audience.com");
        settings.ExpirationInMinutes.Should().Be(120);
        settings.RefreshTokenExpirationInDays.Should().Be(14);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("a-very-long-secret-key-that-should-still-be-accepted")]
    public void JwtSettings_ShouldAcceptVariousSecretLengths(string secret)
    {
        // Arrange & Act
        var settings = new JwtSettings { Secret = secret };

        // Assert
        settings.Secret.Should().Be(secret);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(1440)] // 24 hours
    public void JwtSettings_ShouldAcceptVariousExpirationTimes(int minutes)
    {
        // Arrange & Act
        var settings = new JwtSettings
        {
            Secret = "test-secret",
            ExpirationInMinutes = minutes
        };

        // Assert
        settings.ExpirationInMinutes.Should().Be(minutes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    public void JwtSettings_ShouldAcceptVariousRefreshTokenExpirationDays(int days)
    {
        // Arrange & Act
        var settings = new JwtSettings
        {
            Secret = "test-secret",
            RefreshTokenExpirationInDays = days
        };

        // Assert
        settings.RefreshTokenExpirationInDays.Should().Be(days);
    }

    [Fact]
    public void JwtSettings_ShouldAllowEmptyIssuerAndAudience()
    {
        // Arrange & Act
        var settings = new JwtSettings
        {
            Secret = "test-secret",
            Issuer = string.Empty,
            Audience = string.Empty
        };

        // Assert
        settings.Issuer.Should().BeEmpty();
        settings.Audience.Should().BeEmpty();
    }

    [Fact]
    public void JwtSettings_ShouldSupportPropertyInitialization()
    {
        // Arrange & Act
        var settings = new JwtSettings
        {
            Secret = "test",
            Issuer = "issuer",
            Audience = "audience",
            ExpirationInMinutes = 90,
            RefreshTokenExpirationInDays = 10
        };

        // Assert - All properties should be independently settable
        settings.Secret.Should().Be("test");
        settings.Issuer.Should().Be("issuer");
        settings.Audience.Should().Be("audience");
        settings.ExpirationInMinutes.Should().Be(90);
        settings.RefreshTokenExpirationInDays.Should().Be(10);
    }
}