namespace ECommerce.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Hash password using BCrypt
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verify password against hash
        /// </summary>
        bool VerifyPassword(string password, string hash);
    }

    // Response DTOs
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserAuthDto User { get; set; }
    }

    public class UserAuthDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
    }
}