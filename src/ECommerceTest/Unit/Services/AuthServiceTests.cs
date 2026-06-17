using ECommerce.Services.Implementation;
using ECommerce.Settings;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerce.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        private readonly AuthService _authService;
        private readonly JwtSettings _jwtSettings;
        private readonly Mock<ILogger<AuthService>> _mockLogger;

        public AuthServiceTests()
        {
            _mockLogger = new Mock<ILogger<AuthService>>();
            _jwtSettings = new JwtSettings
            {
                SecretKey = "your-super-secret-key-that-is-very-long-minimum-32-characters-required",
                Issuer = "EcommerceAPI",
                Audience = "EcommerceAPIClient",
                ExpiryMinutes = 60,
                RefreshTokenExpiryDays = 7
            };

            _authService = new AuthService(_mockLogger.Object, _jwtSettings);
        }

        #region Password Hashing

        [Fact]
        public void HashPassword_WithValidPassword_ReturnsHash()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash = _authService.HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEqual(password, hash);
            Assert.StartsWith("$2a$", hash); // BCrypt hash format
        }

        [Fact]
        public void HashPassword_WithSamePassword_ReturnsDifferentHash()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash1 = _authService.HashPassword(password);
            var hash2 = _authService.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2); // Different salts
        }

        #endregion

        #region Password Verification

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "TestPassword123";
            var hash = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword(password, hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WithWrongPassword_ReturnsFalse()
        {
            // Arrange
            var password = "TestPassword123";
            var wrongPassword = "WrongPassword456";
            var hash = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region JWT Token

        [Fact]
        public void GenerateJwtToken_WithValidData_ReturnsToken()
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var role = "Customer";

            // Act
            var token = _authService.GenerateJwtToken(userId, email, role);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.Contains(".", token); // JWT format: xxx.yyy.zzz
        }

        [Fact]
        public void ValidateJwtToken_WithValidToken_ReturnsValidClaims()
        {
            // Arrange
            var userId = 1;
            var email = "test@example.com";
            var role = "Customer";
            var token = _authService.GenerateJwtToken(userId, email, role);

            // Act
            var result = _authService.ValidateJwtToken(token);

            // Assert
            Assert.True(result.isValid);
            Assert.Equal(userId, result.userId);
            Assert.Equal(email, result.email);
            Assert.Equal(role, result.role);
        }

        [Fact]
        public void ValidateJwtToken_WithInvalidToken_ReturnsInvalid()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var result = _authService.ValidateJwtToken(invalidToken);

            // Assert
            Assert.False(result.isValid);
        }

        #endregion
    }
}