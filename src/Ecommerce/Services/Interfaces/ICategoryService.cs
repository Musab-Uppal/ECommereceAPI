namespace ECommerce.Services.Interfaces
{
    public interface ICategoryService
    {
        // Read operations
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(int pageNumber, int pageSize);
        Task<CategoryDto> GetCategoryByIdAsync(int id);

        // Create operations
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);

        // Update operations
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);

        // Delete operations
        Task<bool> DeleteCategoryAsync(int id);
    }

    // DTOs
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}