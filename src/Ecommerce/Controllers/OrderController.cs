using Microsoft.AspNetCore.Mvc;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
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
        /// Create a new order
        /// POST /api/order?userId=123
        /// Body: { "items": [{ "productId": 1, "quantity": 2, "discount": 0 }] }
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderServiceDto>> CreateOrder([FromQuery] int userId, [FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid or missing userId");
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
        /// Get all orders (Admin only)
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
        /// Get order by ID
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
        /// Get user's orders
        /// GET /api/order/my-orders?userId=123
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<OrderServiceDto>>> GetMyOrders([FromQuery] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("Invalid or missing userId");
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
        /// Update order status (Admin only)
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
        /// Cancel an order and restore stock
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
        /// Get revenue statistics (Admin only)
        /// GET /api/order/stats/revenue
        /// </summary>
        [HttpGet("stats/revenue")]
        [Authorize(Roles ="Admin")]
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

    }
}