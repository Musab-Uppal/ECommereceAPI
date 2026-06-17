using ECommerce.Models;

namespace ECommerce.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        // Read operations
        Task<IEnumerable<Category>> GetAllCategoriesAsync(int pageNumber, int pageSize);
        Task<Category> GetCategoryByIdAsync(int id);
        Task<int> GetTotalCategoriesCountAsync();

        // Create operations
        Task<Category> CreateCategoryAsync(Category category);

        // Update operations
        Task<Category> UpdateCategoryAsync(Category category);

        // Delete operations
        Task<bool> DeleteCategoryAsync(int id);

        // Check operations
        Task<bool> CategoryExistsAsync(int id);
        Task<bool> CategoryNameExistsAsync(string name);
        Task<bool> CategoryNameExistsAsync(string name, int excludeCategoryId);
        Task<bool> HasProductsAsync(int categoryId);
    }
}