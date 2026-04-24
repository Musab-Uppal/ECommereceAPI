using ECommereceAPI.Models;
using ECommereceAPI.Services.Interfaces;
using ECommereceAPI.Repositories.Interfaces;

namespace ECommereceAPI.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all products with pagination and business logic
        /// </summary>
        public async Task<IEnumerable<ProductServiceDto>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            try
            {
                // Validate input
                if (pageNumber < 1 || pageSize < 1)
                {
                    throw new ArgumentException("Page number and size must be greater than 0");
                }

                // Get from repository
                var products = await _productRepository.GetAllProductsAsync(pageNumber, pageSize);

                // Map to DTOs
                return products.Select(p => MapToServiceDto(p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllProductsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get product by ID with business logic
        /// </summary>
        public async Task<ProductServiceDto> GetProductByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("Product ID must be greater than 0");
                }

                var product = await _productRepository.GetProductByIdAsync(id);

                if (product == null)
                {
                    return null;
                }

                return MapToServiceDto(product);
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
        public async Task<IEnumerable<ProductServiceDto>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    throw new ArgumentException("Category ID must be greater than 0");
                }

                // Verify category exists
                var categoryExists = await _productRepository.CategoryExistsAsync(categoryId);
                if (!categoryExists)
                {
                    throw new InvalidOperationException($"Category with ID {categoryId} does not exist");
                }

                var products = await _productRepository.GetProductsByCategoryAsync(categoryId);

                return products.Select(p => MapToServiceDto(p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProductsByCategoryAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new product with validation
        /// </summary>
        public async Task<ProductServiceDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            try
            {
                // Validate input
                ValidateProductInput(createProductDto);

                // Verify category exists
                var categoryExists = await _productRepository.CategoryExistsAsync(createProductDto.CategoryId);
                if (!categoryExists)
                {
                    throw new InvalidOperationException($"Category with ID {createProductDto.CategoryId} does not exist");
                }

                // Create product entity
                var product = new Product
                {
                    Name = createProductDto.Name.Trim(),
                    Description = createProductDto.Description?.Trim(),
                    Price = createProductDto.Price,
                    Stock = createProductDto.Stock,
                    CategoryId = createProductDto.CategoryId
                };

                // Save to repository
                var createdProduct = await _productRepository.CreateProductAsync(product);

                _logger.LogInformation($"Product created: {createdProduct.ProductId} - {createdProduct.Name}");

                return MapToServiceDto(createdProduct);
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
        public async Task<ProductServiceDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                // Verify product exists
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }

                // Update fields if provided
                if (!string.IsNullOrWhiteSpace(updateProductDto.Name))
                {
                    product.Name = updateProductDto.Name.Trim();
                }

                if (!string.IsNullOrWhiteSpace(updateProductDto.Description))
                {
                    product.Description = updateProductDto.Description.Trim();
                }

                if (updateProductDto.Price.HasValue)
                {
                    if (updateProductDto.Price <= 0)
                    {
                        throw new ArgumentException("Price must be greater than 0");
                    }
                    product.Price = updateProductDto.Price.Value;
                }

                if (updateProductDto.Stock.HasValue)
                {
                    if (updateProductDto.Stock < 0)
                    {
                        throw new ArgumentException("Stock cannot be negative");
                    }
                    product.Stock = updateProductDto.Stock.Value;
                }

                if (updateProductDto.CategoryId.HasValue)
                {
                    var categoryExists = await _productRepository.CategoryExistsAsync(updateProductDto.CategoryId.Value);
                    if (!categoryExists)
                    {
                        throw new InvalidOperationException($"Category with ID {updateProductDto.CategoryId} does not exist");
                    }
                    product.CategoryId = updateProductDto.CategoryId.Value;
                }

                // Save changes
                var updatedProduct = await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation($"Product updated: {updatedProduct.ProductId} - {updatedProduct.Name}");

                return MapToServiceDto(updatedProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateProductAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a product with business logic checks
        /// </summary>
        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                // Verify product exists
                var productExists = await _productRepository.ProductExistsAsync(id);
                if (!productExists)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }

                // Check if product is in any order (business logic)
                var isInOrder = await _productRepository.IsProductInAnyOrderAsync(id);
                if (isInOrder)
                {
                    throw new InvalidOperationException("Cannot delete product that has been ordered");
                }

                // Delete product
                var deleted = await _productRepository.DeleteProductAsync(id);

                if (deleted)
                {
                    _logger.LogInformation($"Product deleted: {id}");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteProductAsync: {ex.Message}");
                throw;
            }
        }

        // Helper methods
        private void ValidateProductInput(CreateProductDto createProductDto)
        {
            if (string.IsNullOrWhiteSpace(createProductDto.Name))
            {
                throw new ArgumentException("Product name is required");
            }

            if (createProductDto.Name.Length > 200)
            {
                throw new ArgumentException("Product name cannot exceed 200 characters");
            }

            if (createProductDto.Price <= 0)
            {
                throw new ArgumentException("Price must be greater than 0");
            }

            if (createProductDto.Stock < 0)
            {
                throw new ArgumentException("Stock cannot be negative");
            }

            if (createProductDto.CategoryId <= 0)
            {
                throw new ArgumentException("Valid category ID is required");
            }
        }

        private ProductServiceDto MapToServiceDto(Product product)
        {
            return new ProductServiceDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "Unknown",
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}