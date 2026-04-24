namespace ECommereceAPI.Services.Interfaces
{
    public interface IProductService
    {
        // Product operations
        Task<IEnumerable<ProductServiceDto>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<ProductServiceDto> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductServiceDto>> GetProductsByCategoryAsync(int categoryId);
        Task<ProductServiceDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductServiceDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int id);
    }

    // Service DTOs
    public class ProductServiceDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public int? CategoryId { get; set; }
    }
}