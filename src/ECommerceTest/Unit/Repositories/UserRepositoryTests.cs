using Xunit;
using ECommerce.Repositories.Implementation;
using ECommerce.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Tests.Unit.Repositories
{
    public class UserRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly Mock<ILogger<UserRepository>> _mockLogger;
        private readonly UserRepository _repository;

        public UserRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _mockLogger = new Mock<ILogger<UserRepository>>();
            _repository = new UserRepository(_fixture.Context, _mockLogger.Object);
        }

        #region GetUserByIdAsync

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
        {
            // Act
            var result = await _repository.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal("admin@test.com", result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetUserByEmailAsync

        [Fact]
        public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
        {
            // Act
            var result = await _repository.GetUserByEmailAsync("admin@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("admin@test.com", result.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithInvalidEmail_ReturnsNull()
        {
            // Act
            var result = await _repository.GetUserByEmailAsync("nonexistent@test.com");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UserExistsAsync

        [Fact]
        public async Task UserExistsAsync_WithValidId_ReturnsTrue()
        {
            // Act
            var result = await _repository.UserExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UserExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _repository.UserExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region EmailExistsAsync

        [Fact]
        public async Task EmailExistsAsync_WithExistingEmail_ReturnsTrue()
        {
            // Act
            var result = await _repository.EmailExistsAsync("admin@test.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EmailExistsAsync_WithNonexistentEmail_ReturnsFalse()
        {
            // Act
            var result = await _repository.EmailExistsAsync("new@test.com");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CreateUserAsync

        [Fact]
        public async Task CreateUserAsync_WithValidUser_CreatesUser()
        {
            // Arrange
            var newUser = TestDataFactory.CreateTestUser(
                id: 100,
                email: "newuser@test.com",
                role: "Customer"
            );

            // Act
            var result = await _repository.CreateUserAsync(newUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("newuser@test.com", result.Email);

            // Verify saved
            var savedUser = await _repository.GetUserByIdAsync(100);
            Assert.NotNull(savedUser);
        }

        #endregion

        #region UpdateUserAsync

        [Fact]
        public async Task UpdateUserAsync_WithValidUser_UpdatesUser()
        {
            // Arrange
            var user = await _repository.GetUserByIdAsync(2);
            user.FirstName = "UpdatedName";

            // Act
            var result = await _repository.UpdateUserAsync(user);

            // Assert
            Assert.Equal("UpdatedName", result.FirstName);
        }

        #endregion

        #region DeleteUserAsync

        [Fact]
        public async Task DeleteUserAsync_WithValidId_DeletesUser()
        {
            // Arrange
            var newUser = TestDataFactory.CreateTestUser(id: 101, email: "delete@test.com");
            await _repository.CreateUserAsync(newUser);

            // Act
            var result = await _repository.DeleteUserAsync(101);

            // Assert
            Assert.True(result);

            // Verify deletion
            var deletedUser = await _repository.GetUserByIdAsync(101);
            Assert.Null(deletedUser);
        }

        #endregion

        #region GetAllUsersAsync

        [Fact]
        public async Task GetAllUsersAsync_WithValidPagination_ReturnsUsers()
        {
            // Act
            var result = await _repository.GetAllUsersAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.Count() >= 2);
        }

        #endregion
    }
}