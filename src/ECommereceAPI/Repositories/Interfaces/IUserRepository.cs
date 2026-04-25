using ECommereceAPI.Models;

namespace ECommereceAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // Read operations
        Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<int> GetTotalUsersCountAsync();

        // Create operations
        Task<User> CreateUserAsync(User user);

        // Update operations
        Task<User> UpdateUserAsync(User user);

        // Delete operations
        Task<bool> DeleteUserAsync(int id);

        // Check operations
        Task<bool> UserExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> EmailExistsAsync(string email, int excludeUserId);
    }
}