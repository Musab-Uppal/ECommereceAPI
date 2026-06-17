using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Services.Interfaces;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        #region Dashboard

        /// <summary>
        /// Get dashboard statistics for admin
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _adminService.GetDashboardStatsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard stats");
                return StatusCode(500, new { success = false, message = "Error fetching dashboard statistics" });
            }
        }

        #endregion

        #region Users Management

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(int page = 1, int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Invalid pagination parameters" });

                var result = await _adminService.GetAllUsersAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, new { success = false, message = "Error fetching users" });
            }
        }

        /// <summary>
        /// Change user role
        /// </summary>
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] ChangeRoleDto dto)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new { success = false, message = "Invalid user ID" });

                if (string.IsNullOrEmpty(dto.NewRole) || (dto.NewRole != "Admin" && dto.NewRole != "Customer"))
                    return BadRequest(new { success = false, message = "Invalid role" });

                var user = await _adminService.ChangeUserRoleAsync(id, dto.NewRole);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found" });

                _logger.LogInformation($"Admin changed user {id} role to {dto.NewRole}");
                return Ok(new { success = true, data = user, message = "Role changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing user role for {id}");
                return StatusCode(500, new { success = false, message = "Error changing user role" });
            }
        }

        #endregion

        #region Products Management

        /// <summary>
        /// Get all products with pagination
        /// </summary>
        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts(int page = 1, int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Invalid pagination parameters" });

                var result = await _adminService.GetAllProductsAdminAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return StatusCode(500, new { success = false, message = "Error fetching products" });
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductAdminDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid product data", errors = ModelState });

                var product = await _adminService.CreateProductAsync(dto);
                if (product == null)
                    return BadRequest(new { success = false, message = "Error creating product" });

                _logger.LogInformation($"Admin created product: {product.ProductId}");
                return CreatedAtAction(nameof(CreateProduct), new { id = product.ProductId }, new { success = true, data = product });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { success = false, message = "Error creating product" });
            }
        }

        /// <summary>
        /// Update a product
        /// </summary>
        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductAdminDto dto)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new { success = false, message = "Invalid product ID" });

                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid product data" });

                var product = await _adminService.UpdateProductAsync(id, dto);
                if (product == null)
                    return NotFound(new { success = false, message = "Product not found" });

                _logger.LogInformation($"Admin updated product: {id}");
                return Ok(new { success = true, data = product, message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {id}");
                return StatusCode(500, new { success = false, message = "Error updating product" });
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new { success = false, message = "Invalid product ID" });

                var result = await _adminService.DeleteProductAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Product not found" });

                _logger.LogInformation($"Admin deleted product: {id}");
                return Ok(new { success = true, message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {id}");
                return StatusCode(500, new { success = false, message = "Error deleting product" });
            }
        }

        /// <summary>
        /// Add stock to a product
        /// </summary>
        [HttpPost("products/{id}/add-stock")]
        public async Task<IActionResult> AddStock(int id, [FromBody] StockUpdateDto dto)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new { success = false, message = "Invalid product ID" });

                if (dto.Quantity <= 0)
                    return BadRequest(new { success = false, message = "Quantity must be greater than 0" });

                var product = await _adminService.AddProductStockAsync(id, dto.Quantity);
                if (product == null)
                    return NotFound(new { success = false, message = "Product not found" });

                _logger.LogInformation($"Admin added {dto.Quantity} stock to product {id}");
                return Ok(new { success = true, data = product, message = $"Added {dto.Quantity} units to stock" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding stock to product {id}");
                return StatusCode(500, new { success = false, message = "Error adding stock" });
            }
        }

        /// <summary>
        /// Remove stock from a product
        /// </summary>
        [HttpPost("products/{id}/remove-stock")]
        public async Task<IActionResult> RemoveStock(int id, [FromBody] StockUpdateDto dto)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new { success = false, message = "Invalid product ID" });

                if (dto.Quantity <= 0)
                    return BadRequest(new { success = false, message = "Quantity must be greater than 0" });

                var product = await _adminService.RemoveProductStockAsync(id, dto.Quantity);
                if (product == null)
                    return NotFound(new { success = false, message = "Product not found" });

                _logger.LogInformation($"Admin removed {dto.Quantity} stock from product {id}");
                return Ok(new { success = true, data = product, message = $"Removed {dto.Quantity} units from stock" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing stock from product {id}");
                return StatusCode(500, new { success = false, message = "Error removing stock" });
            }
        }

        /// <summary>
        /// Get low stock products
        /// </summary>
        [HttpGet("products/low-stock")]
        public async Task<IActionResult> GetLowStockProducts()
        {
            try
            {
                var products = await _adminService.GetLowStockProductsAsync();
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching low stock products");
                return StatusCode(500, new { success = false, message = "Error fetching low stock products" });
            }
        }

        #endregion

        #region Categories Management

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _adminService.GetAllCategoriesAsync();
                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                return StatusCode(500, new { success = false, message = "Error fetching categories" });
            }
        }

        /// <summary>
        /// Create a category
        /// </summary>
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid category data" });

                var category = await _adminService.CreateCategoryAsync(dto);
                if (category == null)
                    return BadRequest(new { success = false, message = "Error creating category" });

                _logger.LogInformation($"Admin created category: {category.CategoryId}");
                return CreatedAtAction(nameof(CreateCategory), new { id = category.CategoryId }, new { success = true, data = category });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { success = false, message = "Error creating category" });
            }
        }

        /// <summary>
        /// Update a category
        /// </summary>
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new { success = false, message = "Invalid category ID" });

                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid category data" });

                var category = await _adminService.UpdateCategoryAsync(id, dto);
                if (category == null)
                    return NotFound(new { success = false, message = "Category not found" });

                _logger.LogInformation($"Admin updated category: {id}");
                return Ok(new { success = true, data = category, message = "Category updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category {id}");
                return StatusCode(500, new { success = false, message = "Error updating category" });
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new { success = false, message = "Invalid category ID" });

                var result = await _adminService.DeleteCategoryAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Category not found or has products" });

                _logger.LogInformation($"Admin deleted category: {id}");
                return Ok(new { success = true, message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category {id}");
                return StatusCode(500, new { success = false, message = "Error deleting category" });
            }
        }

        #endregion

        #region Orders Management

        /// <summary>
        /// Get all orders with pagination
        /// </summary>
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders(int page = 1, int pageSize = 10, string? status = null)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Invalid pagination parameters" });

                var result = await _adminService.GetAllOrdersAdminAsync(page, pageSize, status);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return StatusCode(500, new { success = false, message = "Error fetching orders" });
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusAdminDto dto)
        {
            try
            {
                if (id < 1)
                    return BadRequest(new { success = false, message = "Invalid order ID" });

                if (string.IsNullOrEmpty(dto.Status))
                    return BadRequest(new { success = false, message = "Status is required" });

                var order = await _adminService.UpdateOrderStatusAsync(id, dto.Status);
                if (order == null)
                    return NotFound(new { success = false, message = "Order not found" });

                _logger.LogInformation($"Admin updated order {id} status to {dto.Status}");
                return Ok(new { success = true, data = order, message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order {id} status");
                return StatusCode(500, new { success = false, message = "Error updating order status" });
            }
        }

        #endregion
    }
}