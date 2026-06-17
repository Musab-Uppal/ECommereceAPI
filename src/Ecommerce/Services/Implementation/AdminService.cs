using ECommerce.Models;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Services.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<AdminService> _logger;
        private const int LowStockThreshold = 5;

        public AdminService(
            IUserRepository userRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IOrderRepository orderRepository,
            ILogger<AdminService> logger)
        {
            _userRepository = userRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

       

        /// <summary>
        /// Get dashboard statistics for admin
        /// </summary>
        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching dashboard statistics");

                // Get all orders for calculations
                var allOrders = await _orderRepository.GetAllOrdersAsync(1, 10000);

                var deliveredOrders = allOrders
                    .Where(o => o.Status.Equals("delivered", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var salesByProductId = BuildSalesByProductId(deliveredOrders);

                var totalRevenue = deliveredOrders.Sum(o => o.TotalAmount);

                var totalOrders = allOrders.Count();

                var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var ordersThisMonth = deliveredOrders
                    .Count(o => o.OrderDate >= monthStart);

                var revenueThisMonth = deliveredOrders
                    .Where(o => o.OrderDate >= monthStart)
                    .Sum(o => o.TotalAmount);

                // Get recent orders
                var recentOrders = allOrders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new OrderAdminViewDto
                    {
                        OrderId = o.OrderId,
                        UserId = o.UserId,
                        UserEmail = o.User?.Email ?? "Unknown",
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        ItemCount = o.OrderItems?.Count ?? 0,
                        OrderDate = o.OrderDate,
                        Items = o.OrderItems?.Select(oi => new OrderItemDto
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.Product?.Name ?? "Unknown",
                            Quantity = oi.Quantity,
                            Price = oi.UnitPrice
                        }).ToList() ?? new List<OrderItemDto>()
                    })
                    .ToList();

                // Get all products
                var allProducts = await _productRepository.GetAllProductsAsync(1, 10000);
                var totalProducts = allProducts.Count();
                var lowStockProducts = allProducts.Count(p => p.Stock <= LowStockThreshold);

                var productsWithSales = allProducts
                    .Select(p => new
                    {
                        Product = p,
                        TotalSold = GetTotalSold(salesByProductId, p.ProductId)
                    })
                    .ToList();

                // Get top selling products
                var topProducts = productsWithSales
                    .OrderByDescending(p => p.TotalSold)
                    .Take(4)
                    .Select(p => new ProductAdminViewDto
                    {
                        ProductId = p.Product.ProductId,
                        Name = p.Product.Name,
                        Price = p.Product.Price,
                        Stock = p.Product.Stock,
                        MinimumStock = LowStockThreshold,
                        IsLowStock = p.Product.Stock <= LowStockThreshold,
                        ImageUrl = p.Product.ImageUrl,
                        TotalSold = p.TotalSold,
                        CategoryId = p.Product.CategoryId,
                        CategoryName = p.Product.Category?.Name ?? "Uncategorized",
                        CreatedAt = p.Product.CreatedAt,
                        UpdatedAt = p.Product.UpdatedAt,
                        Description = p.Product.Description
                    })
                    .ToList();

                // Get all users
                var allUsers = await _userRepository.GetAllUsersAsync(1, 10000);
                var totalUsers = allUsers.Count();

                return new AdminDashboardStatsDto
                {
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    TotalProducts = totalProducts,
                    TotalUsers = totalUsers,
                    RevenueThisMonth = revenueThisMonth,
                    OrdersThisMonth = ordersThisMonth,
                    LowStockProducts = lowStockProducts,
                    TopSellingProducts = topProducts,
                    RecentOrders = recentOrders
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                throw;
            }
        }

        

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        public async Task<PaginatedResult<UserAdminViewDto>> GetAllUsersAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogInformation($"Fetching users - Page: {page}, PageSize: {pageSize}");

                var users = await _userRepository.GetAllUsersAsync(page, pageSize);
                var allUsers = await _userRepository.GetAllUsersAsync(1, 10000);
                var totalCount = allUsers.Count();

                var userDtos = users.Select(u => new UserAdminViewDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.Phone,
                    Address = u.Address,
                    Role = u.Role,
                    TotalOrders = u.Orders?.Count ?? 0,
                    TotalSpent = u.Orders?.Sum(o => o.TotalAmount) ?? 0,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.UpdatedAt
                }).ToList();

                return new PaginatedResult<UserAdminViewDto>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

     

        /// <summary>
        /// Change user role (Admin or Customer)
        /// </summary>
        public async Task<UserAdminViewDto?> ChangeUserRoleAsync(int userId, string newRole)
        {
            try
            {
                _logger.LogInformation($"Changing user {userId} role to {newRole}");

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User {userId} not found");
                    return null;
                }

                if (newRole != "Admin" && newRole != "Customer")
                {
                    throw new ArgumentException("Invalid role. Must be 'Admin' or 'Customer'");
                }

                user.Role = newRole;
                user.UpdatedAt = DateTime.UtcNow;
                var updatedUser = await _userRepository.UpdateUserAsync(user);

                _logger.LogInformation($"User {userId} role changed to {newRole}");

                return new UserAdminViewDto
                {
                    UserId = updatedUser.UserId,
                    Email = updatedUser.Email,
                    FirstName = updatedUser.FirstName,
                    LastName = updatedUser.LastName,
                    Phone = updatedUser.Phone,
                    Address = updatedUser.Address,
                    Role = updatedUser.Role,
                    TotalOrders = updatedUser.Orders?.Count ?? 0,
                    TotalSpent = updatedUser.Orders?.Sum(o => o.TotalAmount) ?? 0,
                    CreatedAt = updatedUser.CreatedAt,
                    LastLoginAt = updatedUser.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing user role for {userId}");
                throw;
            }
        }

       

        /// <summary>
        /// Get all products with pagination (admin view)
        /// </summary>
        public async Task<PaginatedResult<ProductAdminViewDto>> GetAllProductsAdminAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogInformation($"Fetching products - Page: {page}, PageSize: {pageSize}");

                var products = await _productRepository.GetAllProductsAsync(page, pageSize);
                var allProducts = await _productRepository.GetAllProductsAsync(1, 10000);
                var totalCount = allProducts.Count();

                var allOrders = await _orderRepository.GetAllOrdersAsync(1, 10000);
                var deliveredOrders = allOrders
                    .Where(o => o.Status.Equals("delivered", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var salesByProductId = BuildSalesByProductId(deliveredOrders);

                var productDtos = products.Select(p =>
                {
                    var totalSold = GetTotalSold(salesByProductId, p.ProductId);
                    return new ProductAdminViewDto
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Stock = p.Stock,
                        MinimumStock = LowStockThreshold,
                        IsLowStock = p.Stock <= LowStockThreshold,
                        ImageUrl = p.ImageUrl,
                        TotalSold = totalSold,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category?.Name ?? "Uncategorized",
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    };
                }).ToList();

                return new PaginatedResult<ProductAdminViewDto>
                {
                    Items = productDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin products");
                throw;
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        public async Task<Product?> CreateProductAsync(CreateProductAdminDto dto)
        {
            try
            {
                _logger.LogInformation($"Creating product: {dto.Name}");

                if (string.IsNullOrWhiteSpace(dto.ImageUrl))
                {
                    throw new ArgumentException("Image URL is required");
                }

                var category = await _categoryRepository.GetCategoryByIdAsync(dto.CategoryId);
                if (category == null)
                {
                    throw new KeyNotFoundException($"Category {dto.CategoryId} not found");
                }

                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Stock = dto.Stock,
                    ImageUrl = dto.ImageUrl.Trim(),
                    CategoryId = dto.CategoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdProduct = await _productRepository.CreateProductAsync(product);
                _logger.LogInformation($"Product created: {createdProduct.ProductId}");
                return createdProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        public async Task<Product?> UpdateProductAsync(int productId, UpdateProductAdminDto dto)
        {
            try
            {
                _logger.LogInformation($"Updating product {productId}");

                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning($"Product {productId} not found");
                    return null;
                }

                if (!string.IsNullOrEmpty(dto.Name))
                    product.Name = dto.Name;
                
                if (!string.IsNullOrEmpty(dto.Description))
                    product.Description = dto.Description;
                
                if (dto.Price.HasValue && dto.Price > 0)
                    product.Price = dto.Price.Value;
                
                if (!string.IsNullOrEmpty(dto.ImageUrl))
                    product.ImageUrl = dto.ImageUrl;
                
                if (dto.CategoryId.HasValue && dto.CategoryId > 0)
                {
                    var category = await _categoryRepository.GetCategoryByIdAsync(dto.CategoryId.Value);
                    if (category == null)
                        throw new KeyNotFoundException($"Category {dto.CategoryId} not found");
                    product.CategoryId = dto.CategoryId.Value;
                }
                
                product.UpdatedAt = DateTime.UtcNow;
                var updatedProduct = await _productRepository.UpdateProductAsync(product);
                
                _logger.LogInformation($"Product {productId} updated successfully");
                return updatedProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {productId}");
                throw;
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                _logger.LogInformation($"Deleting product {productId}");

                var result = await _productRepository.DeleteProductAsync(productId);
                if (result)
                    _logger.LogInformation($"Product {productId} deleted successfully");
                else
                    _logger.LogWarning($"Product {productId} not found");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {productId}");
                throw;
            }
        }

        /// <summary>
        /// Add stock to a product
        /// </summary>
        public async Task<Product?> AddProductStockAsync(int productId, int quantity)
        {
            try
            {
                _logger.LogInformation($"Adding {quantity} stock to product {productId}");

                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning($"Product {productId} not found");
                    return null;
                }

                if (quantity <= 0)
                {
                    throw new ArgumentException("Quantity must be greater than 0");
                }

                product.Stock += quantity;
                product.UpdatedAt = DateTime.UtcNow;
                var updatedProduct = await _productRepository.UpdateProductAsync(product);
                
                _logger.LogInformation($"Added {quantity} stock to product {productId}. New stock: {updatedProduct.Stock}");
                return updatedProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding stock to product {productId}");
                throw;
            }
        }

        /// <summary>
        /// Remove stock from a product
        /// </summary>
        public async Task<Product?> RemoveProductStockAsync(int productId, int quantity)
        {
            try
            {
                _logger.LogInformation($"Removing {quantity} stock from product {productId}");

                var product = await _productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning($"Product {productId} not found");
                    return null;
                }

                if (quantity <= 0)
                {
                    throw new ArgumentException("Quantity must be greater than 0");
                }

                if (product.Stock < quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product {productId}. Available: {product.Stock}, Requested: {quantity}");
                }

                product.Stock -= quantity;
                product.UpdatedAt = DateTime.UtcNow;
                var updatedProduct = await _productRepository.UpdateProductAsync(product);
                
                _logger.LogInformation($"Removed {quantity} stock from product {productId}. New stock: {updatedProduct.Stock}");
                return updatedProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing stock from product {productId}");
                throw;
            }
        }

        /// <summary>
        /// Get products with low stock
        /// </summary>
        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching low stock products");

                var products = await _productRepository.GetAllProductsAsync(1, 10000);
                return products
                    .Where(p => p.Stock <= LowStockThreshold)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products");
                throw;
            }
        }

      
        /// <summary>
        /// Get all categories
        /// </summary>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all categories");

                var categories = await _categoryRepository.GetAllCategoriesAsync(1, 10000);
                return categories.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                throw;
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        public async Task<Category?> CreateCategoryAsync(CreateCategoryDto dto)
        {
            try
            {
                _logger.LogInformation($"Creating category: {dto.Name}");

                var category = new Category
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
                _logger.LogInformation($"Category created: {createdCategory.CategoryId}");
                return createdCategory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        /// <summary>
        /// Update a category
        /// </summary>
        public async Task<Category?> UpdateCategoryAsync(int categoryId, UpdateCategoryDto dto)
        {
            try
            {
                _logger.LogInformation($"Updating category {categoryId}");

                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    _logger.LogWarning($"Category {categoryId} not found");
                    return null;
                }

                if (!string.IsNullOrEmpty(dto.Name))
                    category.Name = dto.Name;
                
                if (!string.IsNullOrEmpty(dto.Description))
                    category.Description = dto.Description;

                category.UpdatedAt = DateTime.UtcNow;
                var updatedCategory = await _categoryRepository.UpdateCategoryAsync(category);
                
                _logger.LogInformation($"Category {categoryId} updated successfully");
                return updatedCategory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category {categoryId}");
                throw;
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation($"Deleting category {categoryId}");

                var result = await _categoryRepository.DeleteCategoryAsync(categoryId);
                if (result)
                    _logger.LogInformation($"Category {categoryId} deleted successfully");
                else
                    _logger.LogWarning($"Category {categoryId} not found or has products");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category {categoryId}");
                throw;
            }
        }

      

        /// <summary>
        /// Get all orders with pagination
        /// </summary>
        public async Task<PaginatedResult<OrderAdminViewDto>> GetAllOrdersAdminAsync(int page, int pageSize, string? status = null)
        {
            try
            {
                _logger.LogInformation($"Fetching orders - Page: {page}, PageSize: {pageSize}, Status: {status ?? "all"}");

                var orders = await _orderRepository.GetAllOrdersAsync(page, pageSize);
                var allOrders = await _orderRepository.GetAllOrdersAsync(1, 10000);
                
                if (!string.IsNullOrEmpty(status))
                {
                    orders = orders
                        .Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    allOrders = allOrders
                        .Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                var totalCount = allOrders.Count();

                var orderDtos = orders.Select(o => new OrderAdminViewDto
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    UserEmail = o.User?.Email ?? "Unknown",
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ItemCount = o.OrderItems?.Count ?? 0,
                    OrderDate = o.OrderDate,
                    Items = o.OrderItems?.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? "Unknown",
                        Quantity = oi.Quantity,
                        Price = oi.UnitPrice
                    }).ToList() ?? new List<OrderItemDto>()
                }).ToList();

                return new PaginatedResult<OrderAdminViewDto>
                {
                    Items = orderDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin orders");
                throw;
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public async Task<Order?> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            try
            {
                _logger.LogInformation($"Updating order {orderId} status to {newStatus}");

                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order {orderId} not found");
                    return null;
                }

                // Validate status
                var validStatuses = new[] { "Pending", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Any(s => s.Equals(newStatus, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"Invalid order status: {newStatus}");
                }

                // Validate status transition
                var currentStatus = order.Status.ToLower();
                var targetStatus = newStatus.ToLower();

                bool isValidTransition = (currentStatus, targetStatus) switch
                {
                    ("pending", "shipped") => true,
                    ("pending", "cancelled") => true,
                    ("shipped", "delivered") => true,
                    ("shipped", "cancelled") => true,
                    var (current, target) when current == target => true,
                    _ => false
                };

                if (!isValidTransition)
                {
                    throw new InvalidOperationException(
                        $"Cannot transition from {order.Status} to {newStatus}");
                }

                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;
                var updatedOrder = await _orderRepository.UpdateOrderAsync(order);

                _logger.LogInformation($"Order {orderId} status updated to {newStatus}");
                return updatedOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order {orderId} status");
                throw;
            }
        }

        private static Dictionary<int, int> BuildSalesByProductId(IEnumerable<Order> orders)
        {
            return orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(oi => oi.Quantity));
        }

        private static int GetTotalSold(Dictionary<int, int> salesByProductId, int productId)
        {
            return salesByProductId.TryGetValue(productId, out var totalSold) ? totalSold : 0;
        }
    }
}