using ECommerce.Controllers;
using ECommerce.Services.Interfaces;
using ECommerce.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ECommerce.Tests.Unit.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _controller = new UserController(_mockUserService.Object, _mockLogger.Object);

            // Setup user claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "1"),
                new Claim(ClaimTypes.Email, "test@example.com")
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        #region Register

        [Fact]
        public async Task Register_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Email = "newuser@example.com",
                Password = "TestPass123",
                FirstName = "Test",
                LastName = "User",
                Phone = "03001234567",
                Address = "Test Address"
            };

            var authResponse = new AuthResponse
            {
                Success = true,
                Message = "User registered successfully",
                Token = "test-token",
                User = new UserAuthDto
                {
                    UserId = 1,
                    Email = "newuser@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    Role = "Customer"
                }
            };

            _mockUserService
                .Setup(x => x.RegisterAsync(registerDto))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(createdResult);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Email = "existing@example.com",
                Password = "TestPass123",
                FirstName = "Test",
                LastName = "User",
                Phone = "03001234567",
                Address = "Test Address"
            };

            var authResponse = new AuthResponse
            {
                Success = false,
                Message = "Email already registered"
            };

            _mockUserService
                .Setup(x => x.RegisterAsync(registerDto))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult);
        }

        #endregion

        #region Login

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "TestPass123"
            };

            var authResponse = new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = "test-token",
                User = new UserAuthDto
                {
                    UserId = 1,
                    Email = "test@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    Role = "Customer"
                }
            };

            _mockUserService
                .Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var authResponse = new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password"
            };

            _mockUserService
                .Setup(x => x.LoginAsync(loginDto))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.NotNull(unauthorizedResult);
        }

        #endregion

        #region GetProfile

        [Fact]
        public async Task GetProfile_WithAuthentication_ReturnsOkWithProfile()
        {
            // Arrange
            var userId = 1;
            var profile = new UserProfileDto
            {
                UserId = userId,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Phone = "03001234567",
                Address = "Test Address",
                Role = "Customer",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserService
                .Setup(x => x.GetUserProfileAsync(userId))
                .ReturnsAsync(profile);

            // Act
            var result = await _controller.GetProfile(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }

        #endregion

        #region UpdateProfile

        [Fact]
        public async Task UpdateProfile_WithValidData_ReturnsOkWithUpdatedProfile()
        {
            // Arrange
            var userId = 1;
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "Updated",
                LastName = "Name",
                Phone = "03009876543",
                Address = "Updated Address"
            };

            var updatedProfile = new UserProfileDto
            {
                UserId = userId,
                Email = "test@example.com",
                FirstName = "Updated",
                LastName = "Name",
                Phone = "03009876543",
                Address = "Updated Address",
                Role = "Customer",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserService
                .Setup(x => x.UpdateUserProfileAsync(userId, updateDto))
                .ReturnsAsync(updatedProfile);

            // Act
            var result = await _controller.UpdateProfile(userId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }

        #endregion

        #region ChangePassword

        [Fact]
        public async Task ChangePassword_WithCorrectCurrentPassword_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword456",
                ConfirmPassword = "NewPassword456"
            };

            _mockUserService
                .Setup(x => x.ChangePasswordAsync(userId, changePasswordDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ChangePassword(userId, changePasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task ChangePassword_WithIncorrectCurrentPassword_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword456",
                ConfirmPassword = "NewPassword456"
            };

            _mockUserService
                .Setup(x => x.ChangePasswordAsync(userId, changePasswordDto))
                .ThrowsAsync(new InvalidOperationException("Current password is incorrect"));

            // Act
            var result = await _controller.ChangePassword(userId, changePasswordDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult);
        }

        #endregion

        #region GetAllUsers (Admin)

        [Fact]
        public async Task GetAllUsers_WithValidPagination_ReturnsOkWithUsers()
        {
            // Arrange
            var users = new List<UserProfileDto>
            {
                new UserProfileDto
                {
                    UserId = 1,
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Phone = "03001234567",
                    Address = "Admin Address",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _mockUserService
                .Setup(x => x.GetAllUsersAsync(1, 10))
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
        }

        #endregion

        #region DeleteUser (Admin)

        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            _mockUserService
                .Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }

        #endregion
    }
}