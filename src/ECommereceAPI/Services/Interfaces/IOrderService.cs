namespace ECommereceAPI.Services.Interfaces
{
    public interface IOrderService
    {
        // Create operations
        Task<OrderServiceDto> CreateOrderAsync(int userId, CreateOrderDto createOrderDto);

        // Read operations
        Task<IEnumerable<OrderServiceDto>> GetAllOrdersAsync(int pageNumber, int pageSize);
        Task<OrderServiceDto> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderServiceDto>> GetUserOrdersAsync(int userId);
        Task<decimal> GetTotalRevenueAsync();

        // Update operations
        Task<OrderServiceDto> UpdateOrderStatusAsync(int orderId, string newStatus);

        // Delete operations
        Task<bool> CancelOrderAsync(int orderId);
    }

    // Service DTOs
    public class OrderServiceDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemServiceDto> OrderItems { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderItemServiceDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? Discount { get; set; }
        public decimal Subtotal => (UnitPrice * Quantity) - (Discount ?? 0);
    }

    public class CreateOrderDto
    {
        public List<OrderItemInputDto> Items { get; set; } = new();
    }

    public class OrderItemInputDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; } = 0; // Optional discount per item
    }

    public class UpdateOrderStatusDto
    {
        public string NewStatus { get; set; }
    }

    public class RevenueStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}