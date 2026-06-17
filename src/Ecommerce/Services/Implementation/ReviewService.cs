using ECommerce.Models;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;

namespace ECommerce.Services.Implementation
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            ILogger<ReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Add a new review with full validation
        /// </summary>
        public async Task<Review> AddReviewAsync(Review review)
        {
            try
            {
                // Validate input
                ValidateReviewInput(review);

                // Verify product exists
                var productExists = await _productRepository.GetProductByIdAsync(review.ProductId);
                if (productExists == null)
                {
                    throw new KeyNotFoundException($"Product with ID {review.ProductId} not found");
                }

                // Verify user exists
                var userExists = await _userRepository.GetUserByIdAsync(review.UserId);
                if (userExists == null)
                {
                    throw new KeyNotFoundException($"User with ID {review.UserId} not found");
                }

                // Ensure review text is trimmed
                if (!string.IsNullOrWhiteSpace(review.ReviewText))
                {
                    review.ReviewText = review.ReviewText.Trim();
                }

                // Set creation timestamp
                review.CreatedAt = DateTime.UtcNow;

                // Add review
                var addedReview = await _reviewRepository.AddReviewAsync(review);

                _logger.LogInformation($"Review added: ProductId={review.ProductId}, UserId={review.UserId}, Rating={review.Rating}");

                return addedReview;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AddReviewAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a review with validation
        /// </summary>
        public async Task<bool> DeleteReviewAsync(int productId, int userId)
        {
            try
            {
                // Validate IDs
                if (productId <= 0)
                {
                    throw new ArgumentException("Product ID must be greater than 0");
                }

                if (userId <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                // Verify product exists
                var productExists = await _productRepository.GetProductByIdAsync(productId);
                if (productExists == null)
                {
                    throw new KeyNotFoundException($"Product with ID {productId} not found");
                }

                // Verify user exists
                var userExists = await _userRepository.GetUserByIdAsync(userId);
                if (userExists == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                // Delete review
                var result = await _reviewRepository.DeleteReviewAsync(productId, userId);

                if (result)
                {
                    _logger.LogInformation($"Review deleted: ProductId={productId}, UserId={userId}");
                }
                else
                {
                    _logger.LogWarning($"Review not found for deletion: ProductId={productId}, UserId={userId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteReviewAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get reviews by product ID with validation
        /// </summary>
        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
        {
            try
            {
                // Validate ID
                if (productId <= 0)
                {
                    throw new ArgumentException("Product ID must be greater than 0");
                }

                // Verify product exists
                var productExists = await _productRepository.GetProductByIdAsync(productId);
                if (productExists == null)
                {
                    throw new KeyNotFoundException($"Product with ID {productId} not found");
                }

                var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);

                _logger.LogInformation($"Retrieved {reviews.Count()} reviews for Product ID {productId}");

                return reviews;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetReviewsByProductIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get reviews by user ID with validation
        /// </summary>
        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            try
            {
                // Validate ID
                if (userId <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                // Verify user exists
                var userExists = await _userRepository.GetUserByIdAsync(userId);
                if (userExists == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);

                _logger.LogInformation($"Retrieved {reviews.Count()} reviews from User ID {userId}");

                return reviews;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetReviewsByUserIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get total review count for a product with validation
        /// </summary>
        public async Task<int> GetReviewsCountByProductIdAsync(int productId)
        {
            try
            {
                // Validate ID
                if (productId <= 0)
                {
                    throw new ArgumentException("Product ID must be greater than 0");
                }

                // Verify product exists
                var productExists = await _productRepository.GetProductByIdAsync(productId);
                if (productExists == null)
                {
                    throw new KeyNotFoundException($"Product with ID {productId} not found");
                }

                var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId);
                var count = reviews.Count();

                _logger.LogInformation($"Review count for Product ID {productId}: {count}");

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetReviewsCountByProductIdAsync: {ex.Message}");
                throw;
            }
        }

        // Helper method - Validation
        private void ValidateReviewInput(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review), "Review cannot be null");
            }

            if (review.ProductId <= 0)
            {
                throw new ArgumentException("Product ID must be greater than 0");
            }

            if (review.UserId <= 0)
            {
                throw new ArgumentException("User ID must be greater than 0");
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5");
            }

            if (!string.IsNullOrWhiteSpace(review.ReviewText) && review.ReviewText.Length > 1000)
            {
                throw new ArgumentException("Review text cannot exceed 1000 characters");
            }
        }
    }
}
