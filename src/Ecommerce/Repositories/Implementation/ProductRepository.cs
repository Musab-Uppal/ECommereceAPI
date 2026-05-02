using Microsoft.EntityFrameworkCore;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Repositories.Interfaces;

namespace ECommerce.Repositories.Implementation
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all products with pagination
        /// </summary>
        public async Task<IEnumerable<Product>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.ProductId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProductByIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                return await _context.Products
                    .Where(p => p.CategoryId == categoryId)
                    .Include(p => p.Category)
                    .OrderBy(p => p.ProductId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProductsByCategoryAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get total count of products
        /// </summary>
        public async Task<int> GetTotalProductsCountAsync()
        {
            try
            {
                return await _context.Products.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTotalProductsCountAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        public async Task<Product> CreateProductAsync(Product product)
        {
            try
            {
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateProductAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        public async Task<Product> UpdateProductAsync(Product product)
        {
            try
            {
                product.UpdatedAt = DateTime.UtcNow;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateProductAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    return false;
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteProductAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if product exists
        /// </summary>
        public async Task<bool> ProductExistsAsync(int id)
        {
            try
            {
                return await _context.Products.AnyAsync(p => p.ProductId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ProductExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if category exists
        /// </summary>
        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            try
            {
                return await _context.Categories.AnyAsync(c => c.CategoryId == categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CategoryExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if product is in any order
        /// </summary>
        public async Task<bool> IsProductInAnyOrderAsync(int productId)
        {
            try
            {
                return await _context.OrderItems.AnyAsync(oi => oi.ProductId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in IsProductInAnyOrderAsync: {ex.Message}");
                throw;
            }
        }
    }
}