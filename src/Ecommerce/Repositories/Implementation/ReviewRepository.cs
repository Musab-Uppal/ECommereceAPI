using ECommerce.Repositories.Interfaces;
using ECommerce.Data;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;
namespace ECommerce.Repositories.Implementation
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewRepository> _logger;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            try
            {
                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();
                return review;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AddReviewAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
        {
            try
            {
                return await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetReviewsByProductIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Reviews
                    .Where(r => r.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetReviewsByUserIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteReviewAsync(int productId, int userId)
        {
            try
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);
                if (review == null)
                {
                    return false;
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteReviewAsync: {ex.Message}");
                throw;
            }



        }
    }
}