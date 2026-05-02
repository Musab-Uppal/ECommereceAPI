using ECommerce.Models;
using ECommerce.Repositories.Interfaces;

namespace ECommerce.Services.Interfaces
{
    public interface IReviewService
    {
        Task<Review> AddReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(int productId, int userId);
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId);
        Task<int> GetReviewsCountByProductIdAsync(int productId);
    }

    // Request DTOs
    public class CreateReviewDto
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
    }

    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string ReviewText { get; set; }
    }

    // Response DTO
    public class ReviewServiceDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
