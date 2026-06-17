using Xunit;
using ECommerce.Repositories.Implementation;
using ECommerce.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Tests.Unit.Repositories
{
    public class ProductRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly Mock<ILogger<ProductRepository>> _mockLogger;
        private readonly ProductRepository _repository;

        public ProductRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _mockLogger = new Mock<ILogger<ProductRepository>>();
            _repository = new ProductRepository(_fixture.Context, _mockLogger.Object);
        }

        #region GetAllProductsAsync

        [Fact]
        public async Task GetAllProductsAsync_WithValidPagination_ReturnsProducts()
        {
            // Act
            var result = await _repository.GetAllProductsAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.Count() >= 2);
        }

        [Fact]
        public async Task GetAllProductsAsync_WithPage2_ReturnsEmptyIfNoMoreProducts()
        {
            // Act
            var result = await _repository.GetAllProductsAsync(2, 10);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetProductByIdAsync

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
        {
            // Act
            var result = await _repository.GetProductByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ProductId);
            Assert.Equal("Test Product 1", result.Name);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetProductByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ProductExistsAsync

        [Fact]
        public async Task ProductExistsAsync_WithValidId_ReturnsTrue()
        {
            // Act
            var result = await _repository.ProductExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ProductExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _repository.ProductExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CreateProductAsync

        [Fact]
        public async Task CreateProductAsync_WithValidProduct_CreatesProduct()
        {
            // Arrange
            var newProduct = TestDataFactory.CreateTestProduct(
                id: 100,
                name: "New Test Product",
                price: 5000
            );

            // Act
            var result = await _repository.CreateProductAsync(newProduct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Test Product", result.Name);
            Assert.Equal(5000, result.Price);

            // Verify it was saved
            var savedProduct = await _repository.GetProductByIdAsync(100);
            Assert.NotNull(savedProduct);
        }

        #endregion

        #region UpdateProductAsync

        [Fact]
        public async Task UpdateProductAsync_WithValidProduct_UpdatesProduct()
        {
            // Arrange
            var product = await _repository.GetProductByIdAsync(1);
            product.Name = "Updated Name";
            product.Price = 1500;

            // Act
            var result = await _repository.UpdateProductAsync(product);

            // Assert
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(1500, result.Price);
        }

        #endregion

        #region DeleteProductAsync

        [Fact]
        public async Task DeleteProductAsync_WithValidId_DeletesProduct()
        {
            // Arrange
            var product = await _repository.GetProductByIdAsync(2);
            Assert.NotNull(product);

            // Act
            var result = await _repository.DeleteProductAsync(2);

            // Assert
            Assert.True(result);

            // Verify deletion
            var deletedProduct = await _repository.GetProductByIdAsync(2);
            Assert.Null(deletedProduct);
        }

        [Fact]
        public async Task DeleteProductAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _repository.DeleteProductAsync(999);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CategoryExistsAsync

        [Fact]
        public async Task CategoryExistsAsync_WithValidId_ReturnsTrue()
        {
            // Act
            var result = await _repository.CategoryExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CategoryExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _repository.CategoryExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}