using Microsoft.EntityFrameworkCore;
using ECommereceAPI.Data;
using ECommereceAPI.Models;
using ECommereceAPI.Repositories.Interfaces;

namespace ECommereceAPI.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        public async Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await _context.Users
                    .OrderBy(u => u.UserId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllUsersAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<User> GetUserByIdAsync(int id)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserByIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get user by email (for login)
        /// </summary>
        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserByEmailAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get total count of users
        /// </summary>
        public async Task<int> GetTotalUsersCountAsync()
        {
            try
            {
                return await _context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTotalUsersCountAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateUserAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateUserAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteUserAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if user exists
        /// </summary>
        public async Task<bool> UserExistsAsync(int id)
        {
            try
            {
                
                return await _context.Users.AnyAsync(u => u.UserId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in EmailExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if email exists (excluding specific user for update)
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, int excludeUserId)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email == email && u.UserId != excludeUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in EmailExistsAsync (exclude): {ex.Message}");
                throw;
            }
        }
    }
}