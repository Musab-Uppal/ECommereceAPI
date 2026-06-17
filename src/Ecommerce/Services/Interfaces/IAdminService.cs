

using System.ComponentModel.DataAnnotations;
using ECommerce.Models;

namespace ECommerce.Services.Interfaces
{
    public interface IAdminService
    {
       

        Task<AdminDashboardStatsDto> GetDashboardStatsAsync();

       

        Task<PaginatedResult<UserAdminViewDto>> GetAllUsersAsync(int page, int pageSize);



        Task<UserAdminViewDto?> ChangeUserRoleAsync(int userId, string newRole);



        Task<PaginatedResult<ProductAdminViewDto>> GetAllProductsAdminAsync(int page, int pageSize);

        Task<Product?> CreateProductAsync(CreateProductAdminDto dto);

        Task<Product?> UpdateProductAsync(int productId, UpdateProductAdminDto dto);

        Task<bool> DeleteProductAsync(int productId);

        Task<Product?> AddProductStockAsync(int productId, int quantity);

        Task<Product?> RemoveProductStockAsync(int productId, int quantity);

        Task<IEnumerable<Product>> GetLowStockProductsAsync();

      

        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        Task<Category?> CreateCategoryAsync(CreateCategoryDto dto);

        Task<Category?> UpdateCategoryAsync(int categoryId, UpdateCategoryDto dto);

        Task<bool> DeleteCategoryAsync(int categoryId);

    
        Task<PaginatedResult<OrderAdminViewDto>> GetAllOrdersAdminAsync(int page, int pageSize, string? status = null);

        Task<Order?> UpdateOrderStatusAsync(int orderId, string newStatus);

      
    }



    public class AdminDashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int OrdersThisMonth { get; set; }
        public int LowStockProducts { get; set; }
        public IEnumerable<ProductAdminViewDto>? TopSellingProducts { get; set; }
        public IEnumerable<OrderAdminViewDto>? RecentOrders { get; set; }
    }

    public class UserAdminViewDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
    }

    public class ProductAdminViewDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int MinimumStock { get; set; }
        public bool IsLowStock { get; set; }
        public string? ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderAdminViewDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public DateTime OrderDate { get; set; }
        public IEnumerable<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateProductAdminDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }

    public class UpdateProductAdminDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? MinimumStock { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
    }

    public class StockUpdateDto
    {
        public int Quantity { get; set; }
    }

 


    public class ChangeRoleDto
    {
        public string NewRole { get; set; } = string.Empty; // "Admin" or "Customer"
    }

    public class UpdateOrderStatusAdminDto
    {
        public string Status { get; set; } = string.Empty; // "Pending", "Shipped", "Delivered", "Cancelled"
    }

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

}