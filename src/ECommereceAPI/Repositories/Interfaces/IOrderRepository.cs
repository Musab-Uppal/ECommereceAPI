using ECommereceAPI.Models;

namespace ECommereceAPI.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        // Read operations
        Task<IEnumerable<Order>> GetAllOrdersAsync(int pageNumber, int pageSize);
        Task<Order> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        Task<int> GetTotalOrdersCountAsync();
        Task<decimal> GetTotalRevenueAsync();

        // Create operations
        Task<Order> CreateOrderAsync(Order order);
        Task<OrderItem> AddOrderItemAsync(OrderItem orderItem);

        // Update operations
        Task<Order> UpdateOrderAsync(Order order);
        Task<Order> UpdateOrderStatusAsync(int orderId, string newStatus);

        // Delete operations
        Task<bool> DeleteOrderAsync(int id);

        // Check operations
        Task<bool> OrderExistsAsync(int id);
        Task<bool> UserExistsAsync(int userId);
        Task<Product> GetProductWithStockAsync(int productId);
        Task<OrderItem> GetOrderItemAsync(int orderId, int productId);
    }
}