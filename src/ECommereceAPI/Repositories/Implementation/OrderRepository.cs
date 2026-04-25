using Microsoft.EntityFrameworkCore;
using ECommereceAPI.Data;
using ECommereceAPI.Models;
using ECommereceAPI.Repositories.Interfaces;

namespace ECommereceAPI.Repositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(ApplicationDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all orders with pagination
        /// </summary>
        public async Task<IEnumerable<Order>> GetAllOrdersAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllOrdersAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get order by ID with all details
        /// </summary>
        public async Task<Order> GetOrderByIdAsync(int id)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == id);
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
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrdersByUserIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get total count of orders
        /// </summary>
        public async Task<int> GetTotalOrdersCountAsync()
        {
            try
            {
                return await _context.Orders.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTotalOrdersCountAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get total revenue from all orders
        /// </summary>
        public async Task<decimal> GetTotalRevenueAsync()
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.Status == "Delivered" || o.Status == "Completed")
                    .SumAsync(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTotalRevenueAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        public async Task<Order> CreateOrderAsync(Order order)
        {
            try
            {
                order.OrderDate = DateTime.UtcNow;
                order.CreatedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateOrderAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Add an item to an order
        /// </summary>
        public async Task<OrderItem> AddOrderItemAsync(OrderItem orderItem)
        {
            try
            {
                orderItem.CreatedAt = DateTime.UtcNow;

                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();

                return orderItem;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AddOrderItemAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing order
        /// </summary>
        public async Task<Order> UpdateOrderAsync(Order order)
        {
            try
            {
                order.UpdatedAt = DateTime.UtcNow;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateOrderAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update order status with validation
        /// </summary>
        public async Task<Order> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);

                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {orderId} not found");
                }

                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateOrderStatusAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete (cancel) an order
        /// </summary>
        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    return false;
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteOrderAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if order exists
        /// </summary>
        public async Task<bool> OrderExistsAsync(int id)
        {
            try
            {
                return await _context.Orders.AnyAsync(o => o.OrderId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OrderExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if user exists
        /// </summary>
        public async Task<bool> UserExistsAsync(int userId)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get product with current stock
        /// </summary>
        public async Task<Product> GetProductWithStockAsync(int productId)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProductWithStockAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get specific order item
        /// </summary>
        public async Task<OrderItem> GetOrderItemAsync(int orderId, int productId)
        {
            try
            {
                return await _context.OrderItems
                    .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.ProductId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetOrderItemAsync: {ex.Message}");
                throw;
            }
        }
    }
}