using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommereceAPI.Services.Interfaces;

namespace ECommereceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all order endpoints
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new order (Customer endpoint - requires authentication)
        /// POST /api/order
        /// Body: { "items": [{ "productId": 1, "quantity": 2, "discount": 0 }] }
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderServiceDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                // Get userId from JWT token claims
                int userId = GetUserIdFromToken();

                if (userId <= 0)
                {
                    return Unauthorized("Invalid or missing authentication");
                }

                var order = await _orderService.CreateOrderAsync(userId, createOrderDto);

                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
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
                _logger.LogError($"Error creating order: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get all orders (Admin only - requires authentication)
        /// GET /api/order?pageNumber=1&pageSize=10
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderServiceDto>>> GetAllOrders(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);

                if (orders == null || !orders.Any())
                {
                    return NotFound("No orders found");
                }

                return Ok(orders);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid argument: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching orders: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get order by ID (Requires authentication)
        /// GET /api/order/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderServiceDto>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found");
                }

                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid argument: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching order: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get current user's orders (Requires authentication)
        /// GET /api/order/my-orders
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<OrderServiceDto>>> GetMyOrders()
        {
            try
            {
                // Get userId from JWT token
                int userId = GetUserIdFromToken();

                if (userId <= 0)
                {
                    return Unauthorized("Invalid or missing authentication");
                }

                var orders = await _orderService.GetUserOrdersAsync(userId);

                return Ok(orders);
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
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user orders: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update order status (Admin only - requires authentication)
        /// PUT /api/order/{id}/status
        /// Body: { "newStatus": "Shipped" }
        /// Valid transitions: Pending → Shipped → Delivered
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderServiceDto>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateStatusDto)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, updateStatusDto.NewStatus);

                return Ok(order);
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
                _logger.LogError($"Error updating order status: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cancel an order and restore stock (Requires authentication)
        /// DELETE /api/order/{id}
        /// Only Pending orders can be cancelled
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> CancelOrder(int id)
        {
            try
            {
                var cancelled = await _orderService.CancelOrderAsync(id);

                if (!cancelled)
                {
                    return NotFound($"Order with ID {id} not found");
                }

                return Ok(new { message = $"Order with ID {id} cancelled successfully. Stock has been restored." });
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
                _logger.LogError($"Error cancelling order: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get revenue statistics (Admin only - requires authentication)
        /// GET /api/order/stats/revenue
        /// </summary>
        [HttpGet("stats/revenue")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RevenueStatsDto>> GetRevenueStats()
        {
            try
            {
                var totalRevenue = await _orderService.GetTotalRevenueAsync();

                // Note: You could fetch more stats from repository if needed
                var stats = new RevenueStatsDto
                {
                    TotalRevenue = totalRevenue,
                    // Additional stats would be added here
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching revenue stats: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // Helper method to extract user ID from JWT token
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return 0;
            }

            return userId;
        }
    }
}