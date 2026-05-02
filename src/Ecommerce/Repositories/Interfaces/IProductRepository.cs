using ECommerce.Models;

namespace ECommerce.Repositories.Interfaces
{
    public interface IProductRepository
    {
        // Read operations
        Task<IEnumerable<Product>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<Product> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<int> GetTotalProductsCountAsync();

        // Create operations
        Task<Product> CreateProductAsync(Product product);

        // Update operations
        Task<Product> UpdateProductAsync(Product product);

        // Delete operations
        Task<bool> DeleteProductAsync(int id);

        // Check operations
        Task<bool> ProductExistsAsync(int id);
        Task<bool> CategoryExistsAsync(int categoryId);
        Task<bool> IsProductInAnyOrderAsync(int productId);
    }
}