using Microsoft.AspNetCore.Mvc;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new review
        /// POST /api/review
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ReviewServiceDto>> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            try
            {
                if (createReviewDto == null)
                {
                    return BadRequest("Review data is required");
                }

                var review = new ECommerce.Models.Review
                {
                    ProductId = createReviewDto.ProductId,
                    UserId = createReviewDto.UserId,
                    Rating = createReviewDto.Rating,
                    ReviewText = createReviewDto.ReviewText,
                    CreatedAt = DateTime.UtcNow
                };

                var createdReview = await _reviewService.AddReviewAsync(review);

                var responseDto = new ReviewServiceDto
                {
                    ProductId = createdReview.ProductId,
                    ProductName = createdReview.Product?.Name,
                    UserId = createdReview.UserId,
                    UserEmail = createdReview.User?.Email,
                    Rating = createdReview.Rating,
                    ReviewText = createdReview.ReviewText,
                    CreatedAt = createdReview.CreatedAt
                };

                return CreatedAtAction(nameof(GetReviewsByProduct), new { productId = createdReview.ProductId }, responseDto);
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
                _logger.LogError($"Error creating review: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get reviews for a product
        /// GET /api/review/product/{productId}
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewServiceDto>>> GetReviewsByProduct(int productId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);

                if (reviews == null || !reviews.Any())
                {
                    return Ok(new List<ReviewServiceDto>());
                }

                var reviewDtos = reviews.Select(r => new ReviewServiceDto
                {
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name,
                    UserId = r.UserId,
                    UserEmail = r.User?.Email,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    CreatedAt = r.CreatedAt
                }).ToList();

                return Ok(reviewDtos);
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
                _logger.LogError($"Error fetching reviews: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get reviews by a specific user
        /// GET /api/review/user/{userId}
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ReviewServiceDto>>> GetReviewsByUser(int userId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);

                if (reviews == null || !reviews.Any())
                {
                    return Ok(new List<ReviewServiceDto>());
                }

                var reviewDtos = reviews.Select(r => new ReviewServiceDto
                {
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name,
                    UserId = r.UserId,
                    UserEmail = r.User?.Email,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    CreatedAt = r.CreatedAt
                }).ToList();

                return Ok(reviewDtos);
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
                _logger.LogError($"Error fetching user reviews: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get review count for a product
        /// GET /api/review/product/{productId}/count
        /// </summary>
        [HttpGet("product/{productId}/count")]
        public async Task<ActionResult<int>> GetReviewCount(int productId)
        {
            try
            {
                var count = await _reviewService.GetReviewsCountByProductIdAsync(productId);
                return Ok(count);
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
                _logger.LogError($"Error fetching review count: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a review
        /// DELETE /api/review?productId=1&userId=1
        /// </summary>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteReview([FromQuery] int productId, [FromQuery] int userId)
        {
            try
            {
                if (productId <= 0 || userId <= 0)
                {
                    return BadRequest("Valid productId and userId are required");
                }

                var success = await _reviewService.DeleteReviewAsync(productId, userId);

                if (!success)
                {
                    return NotFound("Review not found");
                }

                return Ok(new { message = "Review deleted successfully" });
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
                _logger.LogError($"Error deleting review: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
