using ECommereceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ECommereceAPI.Controllers
{
    [ApiController]
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetOrders(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrders: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving orders.");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound("Order not found.");
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrderById: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the order.");
            }
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersByUserId(int userId)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrdersByUserId: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving orders.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(int userId, [FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                var createdOrder = await _orderService.CreateOrderAsync(userId, createOrderDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.OrderId }, createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateOrder: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the order.");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderStatusAsync(orderId, updateOrderStatusDto.NewStatus);
                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateOrderStatus: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }


        [HttpDelete("{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var result = await _orderService.CancelOrderAsync(orderId);
                if (!result)
                {
                    return NotFound("Order not found or could not be canceled.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CancelOrder: {ex.Message}");
                return StatusCode(500, "An error occurred while canceling the order.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            try
            {
                var totalRevenue = await _orderService.GetTotalRevenueAsync();
                return Ok(new { TotalRevenue = totalRevenue });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTotalRevenue: {ex.Message}");
                return StatusCode(500, "An error occurred while calculating total revenue.");
            }
        }


    }
}
