using ECommerce.Models;
namespace ECommerce.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        public Task<Review> AddReviewAsync(Review review);
        public Task<bool> DeleteReviewAsync(int ProductId,int UserId);

        public Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int ProductId);

        public Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int UserId);

    }
}
