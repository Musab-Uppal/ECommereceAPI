using Microsoft.AspNetCore.Mvc;
using ECommereceAPI.Services.Interfaces;

namespace ECommereceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Get all products with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default 1)</param>
        /// <param name="pageSize">Items per page (default 10)</param>
        /// <returns>List of products</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductServiceDto>>> GetAllProducts(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var products = await _productService.GetAllProductsAsync(pageNumber, pageSize);
                
                if (products == null || !products.Any())
                {
                    return NotFound("No products found");
                }

                return Ok(products);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid argument: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching products: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductServiceDto>> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid argument: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching product: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>List of products in category</returns>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductServiceDto>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);

                return Ok(products);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid argument: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching products by category: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new product (Admin only)
        /// </summary>
        /// <param name="createProductDto">Product details</param>
        /// <returns>Created product</returns>
        [HttpPost]
        public async Task<ActionResult<ProductServiceDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(createProductDto);

                return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid argument: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating product: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update a product (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="updateProductDto">Updated product details</param>
        /// <returns>Updated product</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductServiceDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, updateProductDto);

                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid argument: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a product (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);

                if (!success)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                return Ok(new { message = $"Product with ID {id} deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Not found: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Invalid operation: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}