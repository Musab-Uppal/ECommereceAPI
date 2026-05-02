using ECommerce.Models;
using ECommerce.Services.Interfaces;
using ECommerce.Repositories.Interfaces;
namespace ECommerce.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IAuthService authService,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user (anyone can register as Customer)
        /// </summary>
        public async Task<AuthResponse> RegisterAsync(RegisterUserDto registerDto)
        {
            try
            {
                // Validate input
                ValidateRegisterInput(registerDto);

                // Check if email already exists
                var emailExists = await _userRepository.EmailExistsAsync(registerDto.Email);
                if (emailExists)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Email already registered"
                    };
                }

                // Hash password
                var passwordHash = _authService.HashPassword(registerDto.Password);

                // Create user
                var user = new User
                {
                    Email = registerDto.Email.ToLower().Trim(),
                    PasswordHash = passwordHash,
                    FirstName = registerDto.FirstName.Trim(),
                    LastName = registerDto.LastName.Trim(),
                    Phone = registerDto.Phone.Trim(),
                    Address = registerDto.Address.Trim(),
                    Role = "Customer", // All new registrations are customers
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateUserAsync(user);

                _logger.LogInformation($"User registered: {createdUser.Email}");

                return new AuthResponse
                {
                    Success = true,
                    Message = "User registered successfully",
                    User = new UserAuthDto
                    {
                        UserId = createdUser.UserId,
                        Email = createdUser.Email,
                        FirstName = createdUser.FirstName,
                        LastName = createdUser.LastName,
                        Role = createdUser.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in RegisterAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Login user with email and password
        /// </summary>
        public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Email and password are required"
                    };
                }

                // Find user by email
                var user = await _userRepository.GetUserByEmailAsync(loginDto.Email.ToLower().Trim());

                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Verify password
                if (!_authService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Generate JWT token
                var token = _authService.GenerateJwtToken(user.UserId, user.Email, user.Role);

                _logger.LogInformation($"User logged in: {user.Email}");

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = new UserAuthDto
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in LoginAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return null;
                }

                return MapToProfileDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserProfileAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update current user's profile
        /// </summary>
        public async Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }
                updateUserDetails(user, updateDto);

                // Update fields
              

                user.UpdatedAt = DateTime.UtcNow;

                var updatedUser = await _userRepository.UpdateUserAsync(user);

                _logger.LogInformation($"User profile updated: {updatedUser.Email}");

                return MapToProfileDto(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateUserProfileAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(changePasswordDto.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(changePasswordDto.NewPassword) ||
                    string.IsNullOrWhiteSpace(changePasswordDto.ConfirmPassword))
                {
                    throw new ArgumentException("All password fields are required");
                }

                if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                {
                    throw new ArgumentException("New password and confirm password do not match");
                }

                if (changePasswordDto.NewPassword.Length < 6)
                {
                    throw new ArgumentException("New password must be at least 6 characters");
                }
                if(changePasswordDto.NewPassword == changePasswordDto.CurrentPassword)
                {
                    throw new ArgumentException("New password must be different from current password");
                }
                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                // Verify current password
                if (!_authService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    throw new InvalidOperationException("Current password is incorrect");
                }

                // Hash new password
                user.PasswordHash = _authService.HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateUserAsync(user);

                _logger.LogInformation($"Password changed for user: {user.Email}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ChangePasswordAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    throw new ArgumentException("Page number and size must be greater than 0");
                }

                var users = await _userRepository.GetAllUsersAsync(pageNumber, pageSize);
                return users.Select(u => MapToProfileDto(u)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllUsersAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        public async Task<UserProfileDto> GetUserByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                var user = await _userRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    return null;
                }

                return MapToProfileDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserByIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update user (Admin only)
        /// </summary>
        public async Task<UserProfileDto> UpdateUserAsync(int id, AdminUpdateUserDto updateDto)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                var user = await _userRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found");
                }

                // Update fields
                if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
                {
                    user.FirstName = updateDto.FirstName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(updateDto.LastName))
                {
                    user.LastName = updateDto.LastName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Phone))
                {
                    user.Phone = updateDto.Phone.Trim();
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Address))
                {
                    user.Address = updateDto.Address.Trim();
                }

                user.UpdatedAt = DateTime.UtcNow;

                var updatedUser = await _userRepository.UpdateUserAsync(user);

                _logger.LogInformation($"User updated by admin: {updatedUser.Email}");

                return MapToProfileDto(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateUserAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                var user = await _userRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found");
                }
                await _userRepository.DeleteUserAsync(id);               
                _logger.LogInformation($"User deleted: ID={id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteUserAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Change user role (Admin only)
        /// </summary>
        public async Task<UserProfileDto> ChangeUserRoleAsync(int id, string newRole)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                if (string.IsNullOrWhiteSpace(newRole) || (newRole != "Admin" && newRole != "Customer"))
                {
                    throw new ArgumentException("Role must be 'Admin' or 'Customer'");
                }

                var user = await _userRepository.GetUserByIdAsync(id);

                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found");
                }

                user.Role = newRole;
                user.UpdatedAt = DateTime.UtcNow;

                var updatedUser = await _userRepository.UpdateUserAsync(user);

                _logger.LogInformation($"User role changed: {updatedUser.Email} → {newRole}");

                return MapToProfileDto(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ChangeUserRoleAsync: {ex.Message}");
                throw;
            }
        }

        // Helper methods
        private void updateUserDetails(User user, UpdateUserProfileDto updateDto)
        {
            if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
            {
                user.FirstName = updateDto.FirstName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(updateDto.LastName))
            {
                user.LastName = updateDto.LastName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Phone))
            {
                user.Phone = updateDto.Phone.Trim();
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Address))
            {
                user.Address = updateDto.Address.Trim();
            }
        }

        private void ValidateRegisterInput(RegisterUserDto registerDto)
        {
            if (string.IsNullOrWhiteSpace(registerDto.Email))
            {
                throw new ArgumentException("Email is required");
            }

            if (!registerDto.Email.Contains("@"))
            {
                throw new ArgumentException("Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Password))
            {
                throw new ArgumentException("Password is required");
            }

            if (registerDto.Password.Length < 6)
            {
                throw new ArgumentException("Password must be at least 6 characters");
            }

            if (string.IsNullOrWhiteSpace(registerDto.FirstName))
            {
                throw new ArgumentException("First name is required");
            }

            if (string.IsNullOrWhiteSpace(registerDto.LastName))
            {
                throw new ArgumentException("Last name is required");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Phone))
            {
                throw new ArgumentException("Phone is required");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Address))
            {
                throw new ArgumentException("Address is required");
            }
        }

        private UserProfileDto MapToProfileDto(User user)
        {
            return new UserProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}