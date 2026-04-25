//using ECommereceAPI.Services.Interfaces;
namespace ECommereceAPI.Services.Interfaces
{
    public interface IUserService
    {
        // Authentication
        Task<AuthResponse> RegisterAsync(RegisterUserDto registerDto);
        Task<AuthResponse> LoginAsync(LoginDto loginDto);

        // User operations
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);

        // Admin operations
        Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<UserProfileDto> GetUserByIdAsync(int id);
        Task<UserProfileDto> UpdateUserAsync(int id, AdminUpdateUserDto updateDto);
        Task<bool> DeleteUserAsync(int id);
        Task<UserProfileDto> ChangeUserRoleAsync(int id, string newRole);
    }

    // Request DTOs
    public class RegisterUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class UpdateUserProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class AdminUpdateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    // Response DTO
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}