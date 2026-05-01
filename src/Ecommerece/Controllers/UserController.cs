using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user (Public endpoint)
        /// POST /api/user/register
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterUserDto registerDto)
        {
            try
            {
                var response = await _userService.RegisterAsync(registerDto);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return CreatedAtAction(nameof(GetProfile), response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Registration validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Login user (Public endpoint)
        /// POST /api/user/login
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _userService.LoginAsync(loginDto);

                if (!response.Success)
                {
                    return Unauthorized(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging in: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get user's profile
        /// GET /api/user/profile?userId=123
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile([FromQuery] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid or missing userId");
                }

                var profile = await _userService.GetUserProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound("User not found");
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user profile: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update user's profile
        /// PUT /api/user/profile?userId=123
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromQuery] int userId, [FromBody] UpdateUserProfileDto updateDto)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid or missing userId");
                }

                var profile = await _userService.UpdateUserProfileAsync(userId, updateDto);

                return Ok(profile);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating profile: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Change user password
        /// PUT /api/user/change-password?userId=123
        /// </summary>
        [HttpPut("change-password")]
        public async Task<ActionResult> ChangePassword([FromQuery] int userId, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid or missing userId");
                }

                await _userService.ChangePasswordAsync(userId, changePasswordDto);

                return Ok(new { message = "Password changed successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error changing password: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all users (Admin only)
        /// GET /api/user?pageNumber=1&pageSize=10
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetAllUsers(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);

                return Ok(users);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching users: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// GET /api/user/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileDto>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update user (Admin only)
        /// PUT /api/user/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserProfileDto>> UpdateUser(int id, [FromBody] AdminUpdateUserDto updateDto)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, updateDto);

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// DELETE /api/user/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var deleted = await _userService.DeleteUserAsync(id);

                if (!deleted)
                {
                    return NotFound($"User with ID {id} not found");
                }

                return Ok(new { message = $"User with ID {id} deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Change user role (Admin only)
        /// PUT /api/user/{id}/role
        /// Body: { "newRole": "Admin" }
        /// </summary>
        [HttpPut("{id}/role")]
        public async Task<ActionResult<UserProfileDto>> ChangeUserRole(int id, [FromBody] ChangeRoleDto changeRoleDto)
        {
            try
            {
                var user = await _userService.ChangeUserRoleAsync(id, changeRoleDto.NewRole);

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error changing user role: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }

    // Additional DTOs
    public class ChangeRoleDto
    {
        public string NewRole { get; set; }
    }
}