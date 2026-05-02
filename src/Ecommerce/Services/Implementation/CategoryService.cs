using ECommerce.Models;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;

namespace ECommerce.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories with pagination
        /// </summary>
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    throw new ArgumentException("Page number and size must be greater than 0");
                }

                var categories = await _categoryRepository.GetAllCategoriesAsync(pageNumber, pageSize);
                return categories.Select(c => MapToDto(c)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllCategoriesAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("Category ID must be greater than 0");
                }

                var category = await _categoryRepository.GetCategoryByIdAsync(id);

                if (category == null)
                {
                    return null;
                }

                return MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCategoryByIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new category (Admin only)
        /// </summary>
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            try
            {
                // Validate input
                ValidateCreateInput(createCategoryDto);

                // Check if category name already exists
                var nameExists = await _categoryRepository.CategoryNameExistsAsync(createCategoryDto.Name);
                if (nameExists)
                {
                    throw new InvalidOperationException($"Category with name '{createCategoryDto.Name}' already exists");
                }

                var category = new Category
                {
                    Name = createCategoryDto.Name.Trim(),
                    Description = createCategoryDto.Description?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdCategory = await _categoryRepository.CreateCategoryAsync(category);

                _logger.LogInformation($"Category created: {createdCategory.CategoryId} - {createdCategory.Name}");

                return MapToDto(createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateCategoryAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update a category (Admin only)
        /// </summary>
        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("Category ID must be greater than 0");
                }

                var category = await _categoryRepository.GetCategoryByIdAsync(id);

                if (category == null)
                {
                    throw new KeyNotFoundException($"Category with ID {id} not found");
                }

                // Update fields if provided
                if (!string.IsNullOrWhiteSpace(updateCategoryDto.Name))
                {
                    // Check if new name already exists (excluding current category)
                    var nameExists = await _categoryRepository.CategoryNameExistsAsync(updateCategoryDto.Name, id);
                    if (nameExists)
                    {
                        throw new InvalidOperationException($"Category with name '{updateCategoryDto.Name}' already exists");
                    }

                    category.Name = updateCategoryDto.Name.Trim();
                }

                if (!string.IsNullOrWhiteSpace(updateCategoryDto.Description))
                {
                    category.Description = updateCategoryDto.Description.Trim();
                }

                category.UpdatedAt = DateTime.UtcNow;

                var updatedCategory = await _categoryRepository.UpdateCategoryAsync(category);

                _logger.LogInformation($"Category updated: {updatedCategory.CategoryId} - {updatedCategory.Name}");

                return MapToDto(updatedCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateCategoryAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a category (Admin only)
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("Category ID must be greater than 0");
                }

                // Check if category has products
                var hasProducts = await _categoryRepository.HasProductsAsync(id);
                if (hasProducts)
                {
                    throw new InvalidOperationException("Cannot delete category that contains products");
                }

                var deleted = await _categoryRepository.DeleteCategoryAsync(id);

                if (!deleted)
                {
                    throw new KeyNotFoundException($"Category with ID {id} not found");
                }

                _logger.LogInformation($"Category deleted: {id}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteCategoryAsync: {ex.Message}");
                throw;
            }
        }

        // Helper methods
        private void ValidateCreateInput(CreateCategoryDto createCategoryDto)
        {
            if (string.IsNullOrWhiteSpace(createCategoryDto.Name))
            {
                throw new ArgumentException("Category name is required");
            }

            if (createCategoryDto.Name.Length > 100)
            {
                throw new ArgumentException("Category name cannot exceed 100 characters");
            }
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}