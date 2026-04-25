using ECommereceAPI.Models;
using ECommereceAPI.Services.Interfaces;
using ECommereceAPI.Repositories.Interfaces;
namespace ECommereceAPI.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        /// <summary>
        /// Create a new order with items, validate stock, and deduct inventory
        /// </summary>
        public async Task<OrderServiceDto> CreateOrderAsync(int userId, CreateOrderDto createOrderDto)
        {
            try
            {
                // Validate user exists
                var userExists = await _orderRepository.UserExistsAsync(userId);
                if (!userExists)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                // Validate order has items
                if (createOrderDto.Items == null || createOrderDto.Items.Count == 0)
                {
                    throw new ArgumentException("Order must contain at least one item");
                }

                // Validate stock availability and calculate total
                decimal totalAmount = 0;
                var validatedItems = new List<(Product product, OrderItemInputDto item)>();

                foreach (var item in createOrderDto.Items)
                {
                    // Validate product exists and has stock
                    var product = await _productRepository.GetProductByIdAsync(item.ProductId);

                    if (product == null)
                    {
                        throw new KeyNotFoundException($"Product with ID {item.ProductId} not found");
                    }

                    if (item.Quantity <= 0)
                    {
                        throw new ArgumentException($"Quantity for product {product.Name} must be greater than 0");
                    }

                    if (product.Stock < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}");
                    }

                    // Calculate subtotal
                    decimal itemSubtotal = (product.Price * item.Quantity) - (item.Discount ?? 0);
                    totalAmount += itemSubtotal;

                    validatedItems.Add((product, item));
                }

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdOrder = await _orderRepository.CreateOrderAsync(order);

                // Add order items and deduct stock
                foreach (var (product, item) in validatedItems)
                {
                    // Create order item
                    var orderItem = new OrderItem
                    {
                        OrderId = createdOrder.OrderId,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        Discount = item.Discount ?? 0,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _orderRepository.AddOrderItemAsync(orderItem);

                    // Deduct stock from product
                    product.Stock -= item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _productRepository.UpdateProductAsync(product);
                }

                _logger.LogInformation($"Order created: OrderId={createdOrder.OrderId}, UserId={userId}, Total={totalAmount}");

                // Fetch complete order with items
                var completedOrder = await _orderRepository.GetOrderByIdAsync(createdOrder.OrderId);
                return MapToServiceDto(completedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateOrderAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all orders (admin only) with pagination
        /// </summary>
        public async Task<IEnumerable<OrderServiceDto>> GetAllOrdersAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    throw new ArgumentException("Page number and size must be greater than 0");
                }

                var orders = await _orderRepository.GetAllOrdersAsync(pageNumber, pageSize);
                return orders.Select(o => MapToServiceDto(o)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllOrdersAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        public async Task<OrderServiceDto> GetOrderByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("Order ID must be greater than 0");
                }

                var order = await _orderRepository.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return null;
                }

                return MapToServiceDto(order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrderByIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all orders for a specific user
        /// </summary>
        public async Task<IEnumerable<OrderServiceDto>> GetUserOrdersAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("User ID must be greater than 0");
                }

                // Verify user exists
                var userExists = await _orderRepository.UserExistsAsync(userId);
                if (!userExists)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
                return orders.Select(o => MapToServiceDto(o)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserOrdersAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update order status with validation
        /// Valid transitions: Pending → Shipped → Delivered
        /// </summary>
        public async Task<OrderServiceDto> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            try
            {
                if (orderId <= 0)
                {
                    throw new ArgumentException("Order ID must be greater than 0");
                }

                if (string.IsNullOrWhiteSpace(newStatus))
                {
                    throw new ArgumentException("Status is required");
                }

                var order = await _orderRepository.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {orderId} not found");
                }

                // Validate status transition
                ValidateStatusTransition(order.Status, newStatus);

                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;

                var updatedOrder = await _orderRepository.UpdateOrderAsync(order);

                _logger.LogInformation($"Order status updated: OrderId={orderId}, Status={newStatus}");

                return MapToServiceDto(updatedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateOrderStatusAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cancel (delete) an order and restore stock
        /// </summary>
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    throw new ArgumentException("Order ID must be greater than 0");
                }

                var order = await _orderRepository.GetOrderByIdAsync(orderId);

                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {orderId} not found");
                }

                // Can only cancel pending orders
                if (order.Status != "Pending")
                {
                    throw new InvalidOperationException(
                        $"Cannot cancel order with status '{order.Status}'. Only Pending orders can be cancelled.");
                }

                // Restore stock for all items
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetProductByIdAsync(item.ProductId);

                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        product.UpdatedAt = DateTime.UtcNow;
                        await _productRepository.UpdateProductAsync(product);
                    }
                }

                // Delete order
                var deleted = await _orderRepository.DeleteOrderAsync(orderId);

                if (deleted)
                {
                    _logger.LogInformation($"Order cancelled: OrderId={orderId}, Stock restored");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CancelOrderAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get total revenue from completed orders
        /// </summary>
        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                return await _orderRepository.GetTotalRevenueAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTotalRevenueAsync: {ex.Message}");
                throw;
            }
        }

        // Helper methods
        private void ValidateStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid transitions
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "Pending", new List<string> { "Shipped", "Cancelled" } },
                { "Shipped", new List<string> { "Delivered", "Cancelled" } },
                { "Delivered", new List<string> { } }, // Terminal state
                { "Cancelled", new List<string> { } }  // Terminal state
            };

            if (!validTransitions.ContainsKey(currentStatus))
            {
                throw new InvalidOperationException($"Unknown order status: {currentStatus}");
            }

            if (!validTransitions[currentStatus].Contains(newStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot transition from '{currentStatus}' to '{newStatus}'. Valid transitions: {string.Join(", ", validTransitions[currentStatus])}");
            }
        }

        private OrderServiceDto MapToServiceDto(Order order)
        {
            return new OrderServiceDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                UserEmail = order.User?.Email ?? "Unknown",
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemServiceDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Unknown",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Discount = oi.Discount
                }).ToList() ?? new List<OrderItemServiceDto>(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}