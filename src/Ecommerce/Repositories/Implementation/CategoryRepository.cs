using Microsoft.EntityFrameworkCore;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Repositories.Interfaces;

namespace ECommerce.Repositories.Implementation
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories with pagination
        /// </summary>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await _context.Categories
                    .OrderBy(c => c.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
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
        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            try
            {
                return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCategoryByIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get total count of categories
        /// </summary>
        public async Task<int> GetTotalCategoriesCountAsync()
        {
            try
            {
                return await _context.Categories.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTotalCategoriesCountAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateCategoryAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            try
            {
                category.UpdatedAt = DateTime.UtcNow;

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateCategoryAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return false;
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteCategoryAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if category exists
        /// </summary>
        public async Task<bool> CategoryExistsAsync(int id)
        {
            try
            {
                return await _context.Categories.AnyAsync(c => c.CategoryId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CategoryExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if category name exists
        /// </summary>
        public async Task<bool> CategoryNameExistsAsync(string name)
        {
            try
            {
                return await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CategoryNameExistsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if category name exists (excluding specific category)
        /// </summary>
        public async Task<bool> CategoryNameExistsAsync(string name, int excludeCategoryId)
        {
            try
            {
                return await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.CategoryId != excludeCategoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CategoryNameExistsAsync (exclude): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Check if category has products
        /// </summary>
        public async Task<bool> HasProductsAsync(int categoryId)
        {
            try
            {
                return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HasProductsAsync: {ex.Message}");
                throw;
            }
        }
    }
}